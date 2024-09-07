﻿using System.Linq;
using System.Windows;
using System.Windows.Input;

using TomsToolbox.Essentials;
using TomsToolbox.Wpf;

#nullable enable

namespace ICSharpCode.ILSpy.Options
{
	public class OptionsItemViewModel(IOptionPage content) : ObservableObject
	{
		public string Title { get; } = content.Title;

		public IOptionPage Content { get; } = content;
	}

	public class OptionsDialogViewModel : ObservableObject
	{
		private IOptionPage? selectedPage;

		private readonly IOptionPage[] optionPages = App.ExportProvider.GetExports<IOptionPage, IOptionsMetadata>("OptionPages")
			.OrderBy(page => page.Metadata?.Order)
			.Select(item => item.Value)
			.ExceptNullItems()
			.ToArray();

		public OptionsDialogViewModel()
		{
			var settings = SettingsService.Instance.SpySettings;

			foreach (var optionPage in optionPages)
			{
				optionPage.Load(settings);
			}

			OptionPages = optionPages.Select(page => new OptionsItemViewModel(page)).ToArray();
			SelectedPage = optionPages.FirstOrDefault();
		}

		public OptionsItemViewModel[] OptionPages { get; }

		public IOptionPage? SelectedPage {
			get => selectedPage;
			set => SetProperty(ref selectedPage, value);
		}

		public ICommand CommitCommand => new DelegateCommand(Commit);

		public ICommand ResetDefaultsCommand => new DelegateCommand(ResetDefaults);

		private void ResetDefaults()
		{
			if (MessageBox.Show(Properties.Resources.ResetToDefaultsConfirmationMessage, "ILSpy", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				SelectedPage?.LoadDefaults();
			}
		}

		private void Commit()
		{
			SettingsService.Instance.SpySettings.Update(root => {
				foreach (var optionPage in optionPages)
				{
					optionPage.Save(root);
				}
			});
		}
	}
}
