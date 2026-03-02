using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using FileOrganizer.Models;
using FileOrganizer.Engines;
using System.Linq;
using System.Threading.Tasks;

namespace FileOrganizer
{
    public partial class MainWindow : Window
    {
        private List<FileTransferInstruction> _currentPlan = new List<FileTransferInstruction>();
        private readonly AnalysisEngine _analysisEngine;
        private readonly ExecutionEngine _executionEngine;

        public MainWindow()
        {
            InitializeComponent();
            _analysisEngine = new AnalysisEngine(new RoutingEngine(), new PdfHeuristic());
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
                ActionProgressBar.Value = 0;
                ActionProgressBar.Visibility = Visibility.Visible;
                InstructionsDataGrid.ItemsSource = null;

                var progress = new Progress<int>(percent => 
                {
                    ActionProgressBar.Value = percent;
                });

                _currentPlan = await Task.Run(() => _analysisEngine.Analyze(sourcePath, destPath, isCopyMode, progress));
                
                InstructionsDataGrid.ItemsSource = _currentPlan;
                ExecuteButton.IsEnabled = _currentPlan.Any();
                
                if (!_currentPlan.Any())
                {
                    MessageBox.Show("No files found or no actions needed.", "Analysis Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during analysis: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                AnalyzeButton.IsEnabled = true;
                ActionProgressBar.Visibility = Visibility.Collapsed;
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
                ActionProgressBar.Visibility = Visibility.Visible;

                var progress = new Progress<int>(percent => 
                {
                    ActionProgressBar.Value = percent;
                });

                await Task.Run(() => _executionEngine.Execute(_currentPlan, isCopyMode, progress));

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
                ActionProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearPlan()
        {
            _currentPlan.Clear();
            InstructionsDataGrid.ItemsSource = null;
            ExecuteButton.IsEnabled = false;
        }
    }
}