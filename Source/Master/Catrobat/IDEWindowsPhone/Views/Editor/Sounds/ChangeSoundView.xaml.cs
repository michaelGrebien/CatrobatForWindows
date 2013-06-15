﻿using System;
using System.Windows.Controls;
using Catrobat.Core.Objects.Sounds;
using Catrobat.IDECommon.Resources;
using Catrobat.IDECommon.Resources.Editor;
using Catrobat.IDEWindowsPhone.Misc;
using IDEWindowsPhone;
using Microsoft.Phone.Controls;
using System.ComponentModel;
using Microsoft.Phone.Shell;
using Microsoft.Practices.ServiceLocation;
using Catrobat.IDEWindowsPhone.ViewModel.Editor.Sounds;

namespace Catrobat.IDEWindowsPhone.Views.Editor.Sounds
{
    public partial class ChangeSoundView : PhoneApplicationPage
    {
        private readonly ChangeSoundViewModel _changeSoundViewModel = ServiceLocator.Current.GetInstance<ChangeSoundViewModel>();

        private ApplicationBarIconButton _btnSave;

        public ChangeSoundView()
        {
            InitializeComponent();

            BuildApplicationBar();
            (App.Current.Resources["LocalizedStrings"] as LocalizedStrings).PropertyChanged += LanguageChanged;
            _changeSoundViewModel.PropertyChanged += AddNewSoundViewModel_OnPropertyChanged;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            _changeSoundViewModel.ResetViewModel();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                TextBoxSoundName.Focus();
                TextBoxSoundName.SelectAll();
            });

            _btnSave.IsEnabled = _changeSoundViewModel.IsSoundNameValid;
            base.OnNavigatedTo(e);
        }

        private void AddNewSoundViewModel_OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "IsSoundNameValid" && _btnSave != null)
            {
                _btnSave.IsEnabled = _changeSoundViewModel.IsSoundNameValid;
            }
        }

        private void TextBoxSoundName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _changeSoundViewModel.SoundName = TextBoxSoundName.Text;
        }

        #region Appbar

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            _btnSave = new ApplicationBarIconButton(new Uri("/Content/Images/ApplicationBar/dark/appbar.save.rest.png", UriKind.Relative));
            _btnSave.Text = EditorResources.ButtonSave;
            _btnSave.Click += btnSave_Click;
            ApplicationBar.Buttons.Add(_btnSave);

            ApplicationBarIconButton btnCancel = new ApplicationBarIconButton(new Uri("/Content/Images/ApplicationBar/dark/appbar.cancel.rest.png", UriKind.Relative));
            btnCancel.Text = EditorResources.ButtonCancel;
            btnCancel.Click += btnCancel_Click;
            ApplicationBar.Buttons.Add(btnCancel);
        }

        private void LanguageChanged(object sender, PropertyChangedEventArgs e)
        {
            BuildApplicationBar();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _changeSoundViewModel.SaveCommand.Execute(null);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _changeSoundViewModel.CancelCommand.Execute(null);
        }

        #endregion
    }
}
