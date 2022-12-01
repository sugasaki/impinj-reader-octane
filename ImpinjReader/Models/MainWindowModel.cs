﻿using ImpinjOctane;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ImpinjReader.Models
{
    internal class MainWindowModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// PropertyChanged これが無いとメモリリークを起こす
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;


        // Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();


        public ReactiveCollection<TagModel> Result { get; set; } = new ReactiveCollection<TagModel>();


        /// <summary>
        /// デストラクタ
        /// </summary>
        public void Dispose()
        {
            Disposable.Dispose();
        }


        public MainWindowModel()
        {
            // UIスレッド以外からコレクションを操作する為に必要
            BindingOperations.EnableCollectionSynchronization(Result, new object());

            //Result.Add(new TagModel()
            //{
            //    Name = "test",
            //    LastSeenTime = "1"
            //});
        }

    }
}
