using Common.Uhf;
using Impinj.OctaneSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpinjOctane
{
    public class ImpinjReaderController
    {
        private string? hostName { get; set; } = null;
        private ImpinjReader reader = new ImpinjReader();


        public Action<IEnumerable<Common.Uhf.Tag>>? ReceivedTags;


        public Action<string>? onReceiveMessage;

        public Action<ImpinjReaderDTO>? OnConnectionLostEvent;
        public Action<ImpinjReaderDTO>? OnKeepaliveReceivedEvent;

        public Action? OnStartCompleted;
        public Action<string>? OnStartException;
        public Action? OnStopCompleted;
        public Action<string>? OnStopException;


        /// <summary>
        /// リーダーと接続する
        /// </summary>
        /// <param name="host"></param>
        private void ConnectToReader(string host)
        {
            try
            {
                var message = string.Format("Attempting to connect to {0} ({1}).", reader.Name, host);
                //Console.WriteLine(message);
                if (onReceiveMessage != null) onReceiveMessage(message);

                // 接続試行回数の最大値。 (deprecated)
                // 例外が発生するまでの最大接続試行回数。
                //reader.MaxConnectionAttempts = 15; (deprecated)

                // 接続試行がタイムアウトするまでのミリ秒数。
                reader.ConnectTimeout = 6000;

                // リーダーに接続する。
                reader.Connect(host);

                message = "Successfully connected.";
                //Console.WriteLine(message);
                if (onReceiveMessage != null) onReceiveMessage(message);

                // 通信が途絶えている間に見逃したタグのレポートやイベントを送るようにする。
                reader.ResumeEventsAndReports();
            }
            catch (OctaneSdkException e)
            {
               var  message = "Failed to connect.";
                //Console.WriteLine(message);
                if (onReceiveMessage != null) onReceiveMessage(message);
                throw e;
            }
        }

        public void Start(string host, string name = " \"My Reader #1\"")
        {
            hostName = host;
            if (reader.IsConnected) return;

            try
            {
                // readerに名前を付ける。タグのレポートで使用されます。
                reader.Name = name;

                // Connect to the reader.
                ConnectToReader(host);

                // Readerの設定を行う
                var settings = ReaderSetting.StartSetting(reader);

                // 新たに変更した設定を適用します。
                reader.ApplySettings(settings);

                // 設定をリーダの不揮発性メモリに保存します。
                //これにより、電源再投入後も設定を維持することができます。
                reader.SaveSettings();

                // keepaliveメッセージを受信したときに呼び出されるイベントハンドラを指定します。
                reader.KeepaliveReceived += OnKeepaliveReceived;

                //  リーダがキープアライブの送信を停止したときに呼び出されるイベントハンドラを指定します。
                reader.ConnectionLost += OnConnectionLost;

                // TagsReportedイベントハンドラを割り当てる。
                //タグのレポートが利用可能になったときに呼び出されるメソッドを指定します。
                reader.TagsReported += OnTagsReported;

                if (OnStartCompleted != null) OnStartCompleted();
            }
            catch (OctaneSdkException e)
            {
                var message = string.Format("Octane SDK exception: {0}", e.Message);
                // Handle Octane SDK errors.
                //Console.WriteLine(message);
                if (OnStartException != null) OnStartException(message);
            }
            catch (Exception e)
            {
                var message = string.Format("Exception : {0}", e.Message);
                // Handle other .NET errors.
                //Console.WriteLine("Exception : {0}", e.Message);
                if (OnStartException != null) OnStartException(message);
            }
        }

        /// <summary>
        /// リーダー停止
        /// </summary>
        public void Stop()
        {
            try
            {
                if (!reader.IsConnected) return;

                // Stop reading.
                reader.Stop();

                // Disconnect from the reader.
                reader.Disconnect();

                if (OnStopCompleted != null) OnStopCompleted();
            }
            catch (OctaneSdkException e)
            {
                var message = string.Format("Octane SDK exception: {0}", e.Message);
                // Handle Octane SDK errors.
                //Console.WriteLine(message);
                OnStopException?.Invoke(message);
            }
            catch (Exception e)
            {
                var message = string.Format("Exception : {0}", e.Message);
                // Handle other .NET errors.
                //Console.WriteLine("Exception : {0}", e.Message);
                OnStopException?.Invoke(message);
            }
        }


        /// <summary>
        /// このイベントハンドラは、リーダーがキープアライブメッセージの送信を停止した（接続が切れた）場合に呼び出されます。
        /// </summary>
        /// <param name="reader"></param>
        private void OnConnectionLost(ImpinjReader reader)
        {
            // Cleanup
            reader.Disconnect();

            // 再接続を試す
            ConnectToReader(hostName);

            var message = string.Format("Connection lost : {0} ({1})", reader.Name, reader.Address);
            //Console.WriteLine(message);
            if (OnConnectionLostEvent != null) OnConnectionLostEvent(new ImpinjReaderDTO(reader.Name, reader.Address));
            if (onReceiveMessage != null) onReceiveMessage(message);
        }

        /// <summary>
        /// このイベントハンドラは、リーダーからキープアライブメッセージを受信したときに呼び出される。
        /// </summary>
        /// <param name="reader"></param>
        private void OnKeepaliveReceived(ImpinjReader reader)
        {
            var message = string.Format("Keepalive received from {0} ({1})", reader.Name, reader.Address);
            //Console.WriteLine(message);

            if (OnKeepaliveReceivedEvent != null) OnKeepaliveReceivedEvent((new ImpinjReaderDTO(reader.Name, reader.Address)));
            if (onReceiveMessage != null) onReceiveMessage(message);
        }

        /// <summary>
        /// このイベントハンドラは、タグレポートが利用可能になると、非同期に呼び出されます。
        /// レポート内の各タグをループして、データをプリントします。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="report"></param>
        private void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            var now = DateTime.Now;
            var tags = report.Tags.Select(tag => new Common.Uhf.Tag()
            {
                Tid = tag.Tid.ToHexString(),
                AntennaId = tag.AntennaPortNumber,
                Crc = tag.Crc,
                PcBits = tag.PcBits,
                Epc = tag.Epc.ToHexString(),
                PhaseAngle = tag.PhaseAngleInRadians,
                PeakRssi = tag.PeakRssiInDbm,
                ReceivedAt = now
            });

            ReceivedTags?.Invoke(tags);
        }
    }
}
