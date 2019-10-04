﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using ACT.UltraScouter.Config;
using ACT.UltraScouter.Models;
using ACT.UltraScouter.ViewModels.Bases;
using FFXIV.Framework.XIVHelper;

namespace ACT.UltraScouter.ViewModels
{
    public class MyHPViewModel :
        OverlayViewModelBase,
        IOverlayViewModel
    {
        public MyHPViewModel() : this(null, null)
        {
        }

        public MyHPViewModel(
            MyStatus config,
            MyStatusModel model)
        {
            this.Config = config ?? this.GetConfig;
            this.Model = model ?? MyStatusModel.Instance;

            this.RaisePropertyChanged(nameof(Config));
            this.RaisePropertyChanged(nameof(Model));

            this.Initialize();
        }

        protected virtual MyStatus GetConfig => Settings.Instance.MyHP;

        protected virtual string ValuePropertyName => nameof(this.Model.CurrentHP);

        public virtual double Progress => this.Model.CurrentHPRate;

        public override void Initialize()
        {
            this.Model.PropertyChanged += this.Model_PropertyChanged;
            this.Config.PropertyChanged += this.Config_PropertyChanged;
            this.Config.RefreshViewDelegate = this.UpdateBrushes;
        }

        public override void Dispose()
        {
            this.Model.PropertyChanged -= this.Model_PropertyChanged;
            this.Config.PropertyChanged -= this.Config_PropertyChanged;
            base.Dispose();
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == this.ValuePropertyName)
            {
                this.UpdateBrushes();
                this.RaisePropertyChanged(nameof(this.Progress));
            }
        }

        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.Config.IsLock):
                    this.RaisePropertyChanged(nameof(this.ResizeMode));
                    break;
            }
        }

        public virtual MyStatus Config { get; protected set; }

        public virtual MyStatusModel Model { get; protected set; }

        public virtual bool OverlayVisible
        {
            get
            {
                if (!this.Config.Visible)
                {
                    return false;
                }

                if (!this.Config.IsDesignMode)
                {
                    if (this.Config.HideInNotCombat &&
                        !XIVPluginHelper.Instance.InCombat)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public ResizeMode ResizeMode => this.Config.IsLock ?
            ResizeMode.NoResize :
            ResizeMode.CanResizeWithGrip;

        private SolidColorBrush textFill = Brushes.White;

        public SolidColorBrush TextFill
        {
            get => this.textFill;
            set => this.SetProperty(ref this.textFill, value);
        }

        private SolidColorBrush textStroke = Brushes.Navy;

        public SolidColorBrush TextStroke
        {
            get => this.textStroke;
            set => this.SetProperty(ref this.textStroke, value);
        }

        private SolidColorBrush barFill = Brushes.Orange;

        public SolidColorBrush BarFill
        {
            get => this.barFill;
            set => this.SetProperty(ref this.barFill, value);
        }

        private SolidColorBrush barStroke = Brushes.Orange;

        public SolidColorBrush BarStroke
        {
            get => this.barStroke;
            set => this.SetProperty(ref this.barStroke, value);
        }

        private void UpdateBrushes()
        {
            var currentValue = this.Progress * 100;

            var barFillColor = this.Config.ProgressBar.AvailableColor(currentValue);
            this.BarFill = GetBrush(barFillColor);

            this.BarStroke = this.Config.ProgressBar.LinkOutlineColor ?
                this.BarFill :
                GetBrush(this.Config.ProgressBar.OutlineColor);

            this.TextFill = this.Config.LinkFontColorToBarColor ?
                this.BarFill :
                GetBrush(this.Config.DisplayText.Color);

            this.TextStroke = this.Config.LinkFontOutlineColorToBarColor ?
                this.BarFill :
                GetBrush(this.Config.DisplayText.OutlineColor);
        }

        private static readonly Dictionary<Color, SolidColorBrush> CachedBrushes = new Dictionary<Color, SolidColorBrush>(16);

        public static SolidColorBrush GetBrush(
            Color color)
        {
            lock (CachedBrushes)
            {
                if (CachedBrushes.ContainsKey(color))
                {
                    return CachedBrushes[color];
                }

                var brush = new SolidColorBrush(color);
                brush.Freeze();
                CachedBrushes[color] = brush;

                return brush;
            }
        }
    }
}
