using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 実験データを記録し、CSV形式で出力するクラス。
/// 
/// 出力ファイル（Application.persistentDataPath以下）：
///   experiment/{SessionId}/session_info.csv   … セッション情報（1行）
///   experiment/{SessionId}/game_result.csv    … ゲーム成績（スコア・プレイ時間など）
///   experiment/{SessionId}/input_log.csv      … フレーム毎の操作ログ
/// 
/// 【使い方】
///   1. ゲーム開始時に StartSession() を呼ぶ
///   2. 毎フレーム LogInput() を呼ぶ（PlayerControllerのUpdateから）
///   3. ゲーム終了時に EndSession() を呼ぶ
/// </summary>
public class ExperimentDataLogger : MonoBehaviour
{
    public static ExperimentDataLogger Instance { get; private set; }

    // ===== 内部状態 =====
    private string sessionId;
    private InputCondition condition;
    private DateTime sessionStartTime;
    private bool isRecording = false;

    // 操作ログのバッファ
    private readonly List<InputLogEntry> inputLogBuffer = new List<InputLogEntry>();

    // ゲーム成績
    private int courseOutCount = 0;
    private float playTime = 0f;
    private bool isClear = false;

    // ===== 操作ログ1件分 =====
    private struct InputLogEntry
    {
        public float time;
        public int frame;
        public float dirX;
        public float dirZ;
        public float dirMagnitude;
    }

    // ===== Unity ライフサイクル =====

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (isRecording)
        {
            playTime += Time.deltaTime;
        }
    }

    // ===== 公開API =====

    /// <summary>ゲーム開始時に呼ぶ。記録を開始する。</summary>
    public void StartSession()
    {
        if (!ExperimentManager.Instance.IsInitialized)
        {
            Debug.LogError("[ExperimentDataLogger] ExperimentManagerが未初期化です。");
            return;
        }

        sessionId = ExperimentManager.Instance.SessionId;
        condition = ExperimentManager.Instance.Condition;
        sessionStartTime = DateTime.Now;
        playTime = 0f;
        courseOutCount = 0;
        isClear = false;
        inputLogBuffer.Clear();
        isRecording = true;

        Debug.Log($"[ExperimentDataLogger] 記録開始: {sessionId}");
    }

    /// <summary>
    /// 毎フレーム呼ぶ。PlayerControllerのUpdateから呼ぶことを想定。
    /// </summary>
    public void LogInput(Vector3 direction)
    {
        if (!isRecording) return;

        inputLogBuffer.Add(new InputLogEntry
        {
            time = playTime,
            frame = Time.frameCount,
            dirX = direction.x,
            dirZ = direction.z,
            dirMagnitude = direction.magnitude,
        });
    }

    /// <summary>コースアウト発生時に呼ぶ。</summary>
    public void NotifyCourseOut()
    {
        if (!isRecording) return;
        courseOutCount++;
    }

    /// <summary>ゲームクリア時に呼ぶ。</summary>
    public void NotifyClear()
    {
        if (!isRecording) return;
        isClear = true;
    }

    /// <summary>ゲーム終了時に呼ぶ。記録を停止しCSVを出力する。</summary>
    public void EndSession()
    {
        if (!isRecording) return;
        isRecording = false;

        // ScoreManagerからスコアを取得（存在する場合）
        int totalScore = 0;
        if (ScoreManager.Instance != null)
        {
            // ScoreManagerの取得方法はプロジェクトに合わせて調整してください
            // totalScore = ScoreManager.Instance.GetTotalScore();
        }

        Export(totalScore);
        Debug.Log($"[ExperimentDataLogger] 記録終了・出力完了: {sessionId}");
    }

    // ===== CSV出力 =====

    private void Export(int totalScore)
    {
        string baseDir = Path.Combine(Application.persistentDataPath, "experiment", sessionId);
        Directory.CreateDirectory(baseDir);

        WriteSessionInfo(baseDir);
        WriteGameResult(baseDir, totalScore);
        WriteInputLog(baseDir);
    }

    /// session_info.csv：セッション1行のサマリ
    private void WriteSessionInfo(string baseDir)
    {
        var sb = new StringBuilder();
        sb.AppendLine("session_id,condition,start_datetime");
        sb.AppendLine($"{EscapeCsv(sessionId)},{EscapeCsv(condition.ToString())},{EscapeCsv(sessionStartTime.ToString("yyyy-MM-dd HH:mm:ss"))}");
        File.WriteAllBytes(
            Path.Combine(baseDir, "session_info.csv"),
            AddBom(sb.ToString())
        );
    }

    /// game_result.csv：ゲーム成績
    private void WriteGameResult(string baseDir, int totalScore)
    {
        var sb = new StringBuilder();
        sb.AppendLine("session_id,condition,play_time_sec,course_out_count,is_clear,total_score");
        sb.AppendLine(string.Join(",",
            EscapeCsv(sessionId),
            EscapeCsv(condition.ToString()),
            playTime.ToString("F3"),
            courseOutCount.ToString(),
            isClear ? "1" : "0",
            totalScore.ToString()
        ));
        File.WriteAllBytes(
            Path.Combine(baseDir, "game_result.csv"),
            AddBom(sb.ToString())
        );
    }

    /// input_log.csv：フレーム毎の操作ログ
    private void WriteInputLog(string baseDir)
    {
        var sb = new StringBuilder();
        sb.AppendLine("session_id,condition,time_sec,frame,dir_x,dir_z,dir_magnitude");
        string sessionEsc = EscapeCsv(sessionId);
        string condEsc = EscapeCsv(condition.ToString());
        foreach (var entry in inputLogBuffer)
        {
            sb.AppendLine(string.Join(",",
                sessionEsc,
                condEsc,
                entry.time.ToString("F4"),
                entry.frame.ToString(),
                entry.dirX.ToString("F4"),
                entry.dirZ.ToString("F4"),
                entry.dirMagnitude.ToString("F4")
            ));
        }
        File.WriteAllBytes(
            Path.Combine(baseDir, "input_log.csv"),
            AddBom(sb.ToString())
        );
    }

    // ===== ユーティリティ =====

    private static string EscapeCsv(string value)
    {
        if (value == null) return string.Empty;
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static byte[] AddBom(string text)
    {
        var bom = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(text);
        var result = new byte[bom.Length + body.Length];
        Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
        Buffer.BlockCopy(body, 0, result, bom.Length, body.Length);
        return result;
    }
}
