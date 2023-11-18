using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Media;

namespace AnsiWordCounterApp
{
    public partial class Form1 : Form
    {
        private AnsiWordCounter mAnsiWordCounter;

        public Form1()
        {
            InitializeComponent();
            mAnsiWordCounter = new AnsiWordCounter();
        }
        
        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.filenameTextBox.Text = ofd.FileName;
            this.filenameTextBox.SelectionStart = this.filenameTextBox.Text.Length;
            this.filenameTextBox.SelectionLength = 0;
            this.filenameTextBox.Focus();
        }

        private void filenameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                StartCounting();
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            StartCounting();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            CancelCounting();
        }

        private void progressUpdateTimer_Tick(object sender, EventArgs e)
        {
            int progressPercentage = mAnsiWordCounter.GetProgressPercentage();
            if (progressPercentage == -1)
            {
                string errorMessage = mAnsiWordCounter.GetErrorMessage();
                CancelCounting();

                SystemSounds.Asterisk.Play();
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK);
            }
            else if (progressPercentage == 100)
            {
                List<CountedWord> countedWords = mAnsiWordCounter.GetCountedWords();
                mAnsiWordCounter.Reset();

                ShowCountedWords(countedWords);
                this.progressBar.Hide();
                this.progressUpdateTimer.Stop();
            }
            else
            {
                int increment = progressPercentage - this.progressBar.Value;
                if (increment > 0)
                {
                    this.progressBar.Increment(increment);
                    this.progressBar.Increment(-1); // cancel animation for a more accurate progress bar
                    this.progressBar.Increment(1);
                }
            }
        }


        private void StartCounting()
        {
            mAnsiWordCounter.Start(filenameTextBox.Text);

            this.cancelButton.Enabled = true;
            this.progressBar.Value = 0;
            this.progressBar.Show();
            this.progressUpdateTimer.Start();
            ClearCountedWords();
        }

        private void CancelCounting()
        {
            mAnsiWordCounter.Reset();

            this.cancelButton.Enabled = false;
            this.progressBar.Hide();
            this.progressUpdateTimer.Stop();
            ClearCountedWords();
        }

        private void ShowCountedWords(List<CountedWord> countedWords)
        {
            this.dataGridView.DataSource = countedWords;
            this.dataGridView.Columns[0].HeaderCell.Value = "Word";
            this.dataGridView.Columns[1].HeaderCell.Value = "Occurence";
            this.dataGridView.Show();
        }

        private void ClearCountedWords()
        {
            this.dataGridView.Columns.Clear();
            this.dataGridView.DataSource = null;
            this.dataGridView.Hide();
        }
    }
}
