using System;
using System.Windows;
using GoogleTranslateFreeApi;
using Wallanguager.Learning;

namespace Wallanguager.Windows
{
	/// <summary>
	/// Логика взаимодействия для GroupWindow.xaml
	/// </summary>
	public partial class GroupAddWindow : Window
	{
		public PhrasesGroup Group { get; private set; }

		private Predicate<PhrasesGroup> _requirement;

		public GroupAddWindow(Predicate<PhrasesGroup> requirement, PhrasesGroup group = null)
		{
			InitializeComponent();

			_requirement = requirement;

			if (group == null)
				Title = "New Group";
			else
			{
				Title = $"Edit Group {group.GroupName}";
				Group = new PhrasesGroup(group.GroupName, group.GroupTheme, group.ToLanguage, group.FromLanguage);
				UpdateFields();
			}
		}

		private void SubmitButtonClick(object sender, RoutedEventArgs e)
		{
			if (!CheckFieldsOnValide())
			{
				MessageBox.Show("Please fill in all required fields.", "Failed attempt", 
					MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			Language fromLanguage = (Language) groupFromLanguage.SelectedItem;
			Language toLanguage = (Language) groupToLanguage.SelectedItem;

			Group = new PhrasesGroup(groupName.Text, groupTheme.SelectedItem as string, toLanguage, fromLanguage);

			bool? result = _requirement?.Invoke(Group);

			if (result.HasValue && result.Value)
				DialogResult = true;
		}

		private void UpdateFields()
		{
			groupName.Text = Group.GroupName;
			groupTheme.SelectedItem = Group.GroupTheme;
			groupFromLanguage.SelectedItem = Group.FromLanguage;
			groupToLanguage.SelectedItem = Group.ToLanguage;
		}

		private bool CheckFieldsOnValide()
		{
			return groupTheme.SelectedValue != null &&
			       groupFromLanguage.SelectedValue != null &&
			       groupToLanguage.SelectedValue != null &&
			       groupName.Text.Trim() != String.Empty;
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
