using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using NoxHandle.Models;
using static NoxHandle.Models.WinAPI;
using System.Drawing;
using Reactive.Bindings;
using System.IO;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Diagnostics;

namespace NoxHandle.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        public ReactiveProperty<string> WindowTitle { get; } = new ReactiveProperty<string>();
        public ReactiveCollection<string> WindowTitleCollection { get; } = new ReactiveCollection<string>();
        public ReactiveCommand StartCaptureCommand { get; } = new ReactiveCommand();
        public ReactiveProperty<ImageSource> SourceImage { get; } = new ReactiveProperty<ImageSource>();
        public ReactiveProperty<ImageSource> ProcessedImage { get; } = new ReactiveProperty<ImageSource>();
        public ReactiveProperty<bool> IsBinalize { get; } = new ReactiveProperty<bool>();

        public void Initialize()
        {
            StartCaptureCommand.Subscribe(StartCapture);
            
            foreach (var p in Process.GetProcesses())
            {
                if (string.IsNullOrWhiteSpace(p.MainWindowTitle))
                {
                    continue;
                }

                WindowTitleCollection.AddOnScheduler(p.MainWindowTitle);
            }
        }

        private void StartCapture()
        {
            IObservable<Bitmap> observable = new WindowCapture(WindowTitle.Value, WindowCapture.Fps30);
            observable.Subscribe(img =>
            {
                if (IsBinalize.Value)
                {
                    img.Binalize(250);
                }

                DispatcherHelper.UIDispatcher.Invoke(() =>
                {
                    ProcessedImage.Value = WindowCapture.ToImageSource(img);
                });
            });
        }
    }
}
