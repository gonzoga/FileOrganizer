using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using FileOrganizer.Models;
using FileOrganizer.Engines;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FileOrganizer
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<FileTransferInstruction> _currentPlan = new ObservableCollection<FileTransferInstruction>();
        private readonly AnalysisEngine _analysisEngine;
        private readonly ExecutionEngine _executionEngine;

        public MainWindow()
        {
            InitializeComponent();
            _analysisEngine = new AnalysisEngine(new RoutingEngine(), new PdfHeuristic(), new MetadataEngine());
            _executionEngine = new ExecutionEngine();
        }

        private void SelectSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog { Title = "Select Source Folder" };
            if (dialog.ShowDialog() == true)
            {
                SourceTextBox.Text = dialog.FolderName;
                ClearPlan();
            }
        }

        private void SelectDestButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog { Title = "Select Destination Folder" };
            if (dialog.ShowDialog() == true)
            {
                DestTextBox.Text = dialog.FolderName;
                ClearPlan();
            }
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SourceTextBox.Text) || string.IsNullOrWhiteSpace(DestTextBox.Text))
            {
                MessageBox.Show("Please select both source and destination folders.", "Missing Paths", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool isCopyMode = CopyModeCheckBox.IsChecked == true;
                string sourcePath = SourceTextBox.Text;
                string destPath = DestTextBox.Text;

                AnalyzeButton.IsEnabled = false;
                ExecuteButton.IsEnabled = false;
                StatusTextBlock.Text = "Analysis Pending...";
                ActionProgressBar.Visibility = Visibility.Collapsed;
                ProgressLabelText.Visibility = Visibility.Collapsed;
                ProgressPercentText.Visibility = Visibility.Collapsed;

                LoadingOverlay.Visibility = Visibility.Visible;
                InstructionsListView.IsEnabled = false;

                _currentPlan.Clear();
                InstructionsListView.ItemsSource = _currentPlan;

                var instructions = await Task.Run(() => _analysisEngine.Analyze(sourcePath, destPath, isCopyMode));

                _currentPlan = new ObservableCollection<FileTransferInstruction>(instructions);
                InstructionsListView.ItemsSource = _currentPlan;

                ExecuteButton.IsEnabled = _currentPlan.Any();

                if (!_currentPlan.Any())
                {
                    StatusTextBlock.Text = "No Files Found";
                    MessageBox.Show("No files found or no actions needed.", "Analysis Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusTextBlock.Text = "Analysis Complete";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during analysis: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                InstructionsListView.IsEnabled = true;
                AnalyzeButton.IsEnabled = true;
            }
        }

        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentPlan.Any()) return;

            try
            {
                bool isCopyMode = CopyModeCheckBox.IsChecked == true;

                ExecuteButton.IsEnabled = false;
                AnalyzeButton.IsEnabled = false;
                ActionProgressBar.Value = 0;
                StatusTextBlock.Text = "Executing Plan...";
                ActionProgressBar.Visibility = Visibility.Visible;
                ProgressLabelText.Visibility = Visibility.Visible;
                ProgressPercentText.Visibility = Visibility.Visible;
                ProgressPercentText.Text = "0%";

                var progress = new Progress<int>(percent =>
                {
                    ActionProgressBar.Value = percent;
                    ProgressPercentText.Text = $"{percent}%";
                });

                await Task.Run(() => _executionEngine.Execute(_currentPlan.ToList(), isCopyMode, progress));

                StatusTextBlock.Text = "Execution Complete";
                MessageBox.Show("Task Complete.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearPlan();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during execution: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ExecuteButton.IsEnabled = true;
                AnalyzeButton.IsEnabled = true;
            }
            finally
            {
            }
        }

        private void ClearPlan()
        {
            _currentPlan.Clear();
            InstructionsListView.ItemsSource = null;
            ExecuteButton.IsEnabled = false;
            StatusTextBlock.Text = "Pending Analysis";
            ActionProgressBar.Visibility = Visibility.Collapsed;
            ProgressLabelText.Visibility = Visibility.Collapsed;
            ProgressPercentText.Visibility = Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }
    }
}