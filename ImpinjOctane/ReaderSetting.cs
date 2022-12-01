using Impinj.OctaneSdk;

namespace ImpinjOctane
{
    internal static class ReaderSetting
    {
        public static Settings StartSetting(ImpinjReader reader)
        {
            // デフォルトの設定を取得します。
            // これを出発点として、興味のある設定を変更していきます。
            Settings settings = reader.QueryDefaultSettings();

            // AutoStartモードをImmediate(即時)にする
            // 設定が完了したら、すぐにリーダーを起動する。
            // これにより、クライアントが接続されていない状態でも実行できるようになります。
            settings.AutoStart.Mode = AutoStartMode.Immediate;

            // AutoStopモードをNONEにする
            settings.AutoStop.Mode = AutoStopMode.None;

            // Use Advanced GPO to set GPO #1 
            // 高度なGPOを使用して、GPO#1を設定します。
            // when an client (LLRP) connection is present.
            // クライアント (LLRP) 接続があるときに設定します。
            settings.Gpos.GetGpo(1).Mode = GpoMode.LLRPConnectionStatus;

            // すべてのタグレポートにタイムスタンプを含めるようにリーダーに指示します。
            settings.Report.IncludeFirstSeenTime = true;
            settings.Report.IncludeLastSeenTime = true;
            settings.Report.IncludeSeenCount = true;

            // このアプリケーションがリーダーから切断された場合、すべてのタグのレポートとイベントを保持します。
            settings.HoldReportsOnDisconnect = true;

            // キープアライブを有効にします。
            settings.Keepalives.Enabled = true;
            settings.Keepalives.PeriodInMs = 5000;

            // リンクモニターモードを有効にします。
            //連続でKeepalivesメッセージに応答しない場合、リーダーはネットワーク接続を終了します。
            settings.Keepalives.EnableLinkMonitorMode = true;

            // EnableLinkMonitorModeの検知で使用するKeepalivesメッセージに応答しない回数
            settings.Keepalives.LinkDownThreshold = 5; // 5回応答しなければ接続を切断する

            return settings;
        }
    }
}
