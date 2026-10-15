// 발췌: Assets/Editor/ClaudeCommands.cs (전체 약 4,200줄 중 명령 창구 부분)
// AI 세션이 유니티 에디터를 직접 조작하는 브리지 — inbox/<번호>.txt에 명령을 쓰면 에디터가 실행하고 outbox/<번호>.txt에 결과를 쓴다.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// Claude가 사장님 대신 유니티 메뉴를 돌리고 화면을 찍어 결과를 읽는 창구(2026-09-13, 사장님: 「모델배선·맵생성 아직도 내가 해줘야 하냐」).
///
/// 두 가지로 쓴다 — 명령은 같다.
///   · 유니티가 **꺼져 있을 때**(배치모드):
///       Unity -batchmode -projectPath . -executeMethod ClaudeCommands.RunBatch -claudeCommands "menu Tools/아트/모델 배선;menu Tools/맵/원랜디 맵 생성" -quit
///     결과 → ClaudeBridge/outbox/batch.txt
///   · 유니티가 **켜져 있을 때**: ClaudeBridge/inbox/&lt;번호&gt;.txt에 한 줄에 명령 하나를 쓰면 1초 안에 집어 가서 돌리고
///     ClaudeBridge/outbox/&lt;번호&gt;.txt에 결과를 쓴다. (ClaudeBridge/는 Assets 밖이라 임포트되지 않고 git에서도 뺀다)
///
/// 명령
///   refresh                                   에셋 새로 읽기(스크립트 컴파일 포함)
///   menu &lt;메뉴 경로&gt;                          메뉴 실행. 대화상자는 뜨지 않고 내용이 결과에 실린다(EditorGuards.Dialog)
///   shot &lt;파일&gt; px py pz tx ty tz [fov]       열린 씬을 (px,py,pz)에서 (tx,ty,tz)를 보고 찍는다
///   shotobj &lt;파일&gt; &lt;오브젝트 이름&gt; [거리배율]   씬에서 그 이름의 오브젝트를 비스듬히 위에서 찍는다
///   lineup &lt;파일&gt; [폴더] [시작] [개수]        생성된 유닛 프리팹을 격자로 세워 찍는다(누운 유닛 찾기). 칸↔이름 표는 결과에
///   gameshot &lt;파일&gt; [초] [가로x세로] [super:N] [click:&lt;버튼&gt;]  플레이 모드로 들어가 버튼을 눌러 가며 **UI까지** 게임 화면을 찍고 나온다(아래 GameShot 절).
///                                             글씨·UI 판정엔 해상도를 꼭 박는다: gameshot hud.png 3 1920x1080 click:쉬움
///   navlane &lt;레인&gt; [칸 수]                   레인 섬 위 NavMesh 지도(걸을 수 있음·바다 영역·없음). 🔴 맵 배율·복셀·섬 높이를 건드린 뒤엔 반드시 돌린다
/// 사진은 ClaudeBridge/shots/에 PNG로 남는다. 명령마다 그동안의 Debug 로그(경고·오류 포함)가 결과에 실린다.
/// </summary>
[InitializeOnLoad]
public static class ClaudeCommands
{
    const string Folder = "ClaudeBridge";
    static double nextPoll;
    static string currentId;   // Poll이 지금 돌리는 inbox 번호 — gameshot이 결과를 플레이 모드 뒤로 미룰 때 쓴다

    static ClaudeCommands()
    {
        if (Application.isBatchMode) return;
        EditorApplication.update += Poll;
        // gameshot은 플레이 모드를 오가며 도메인이 다시 로드된다(EnterPlayModeOptions에서 리로드를 안 껐다) —
        // 그때마다 이 생성자가 다시 불리므로 진행 상태는 SessionState에 두고 여기서 이어 받는다.
        EditorApplication.update += TickGameShot;
        Application.logMessageReceived += CollectGameShotLog;
        NoteMidPlayReload();
    }

    static void Poll()
    {
        if (EditorApplication.timeSinceStartup < nextPoll) return;
        nextPoll = EditorApplication.timeSinceStartup + 1.0;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        // gameshot이 플레이에서 막 나와 결과를 아직 안 썼으면 다음 명령을 집지 않는다 — 같은 프레임에 Poll이 먼저 돌면
        // 다음 gameshot이 「앞선 gameshot이 아직 안 끝났다」로 거절됐다(2026-09-24 outbox 2048).
        if (LoadGameShot() != null) return;

        string inbox = Path.Combine(Folder, "inbox");
        if (!Directory.Exists(inbox)) return;
        string file = Directory.GetFiles(inbox, "*.txt").OrderBy(f => f, StringComparer.Ordinal).FirstOrDefault();
        if (file == null) return;

        string id = Path.GetFileNameWithoutExtension(file);
        string[] commands = File.ReadAllLines(file);
        File.Delete(file);   // 명령이 컴파일·도메인 리로드를 부르면 이 함수가 다시 불린다 — 두 번 돌지 않게 먼저 지운다

        currentId = id;
        string text;
        try { text = RunAll(commands); }
        finally { currentId = null; }

        // gameshot이 플레이 모드를 예약했으면 결과는 찍고 나온 뒤 한꺼번에 쓴다. 지금 쓰면 기다리는 쪽이
        // 「⏳ 진행 중」만 든 파일을 결과로 읽는다.
        GameShotJob job = LoadGameShot();
        if (job != null && job.id == id && !job.prefixReady)
        {
            job.prefix = text;
            job.prefixReady = true;
            SaveGameShot(job);
            return;
        }
        WriteResult(id, text);
    }

    public static void RunBatch()
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-claudeCommands");
        string commands = index >= 0 && index + 1 < args.Length ? args[index + 1] : "";
        WriteResult("batch", RunAll(commands.Split(';')));
    }

    static void WriteResult(string id, string text)
    {
        string outbox = Path.Combine(Folder, "outbox");
        Directory.CreateDirectory(outbox);
        File.WriteAllText(Path.Combine(outbox, id + ".txt"), text, new UTF8Encoding(false));
    // … (명령별 구현 생략)
}
