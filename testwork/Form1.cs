using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic; // For InputBox

namespace testwork
{
    public partial class Form1 : Form
    {
        Random randomizer = new Random();

        ComboBox difficultyComboBox;
        Label timeLabel;
        Button startButton, prevButton, nextButton;
        System.Windows.Forms.Timer timer1;

        FlowLayoutPanel quizPanel;

        int problemsPerPage = 4;
        int totalPages;
        int currentPage = 0;
        int totalTime;
        int totalProblems;
        int timeLeft;

        int[] operand1;
        int[] operand2;
        string[] operators;
        NumericUpDown[] answerBoxes;

        public Form1()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.Text = "Math Quiz";
            this.Size = new Size(500, 450);
            this.Font = new Font("Segoe UI", 10);

            // Difficulty selector
            Label diffLabel = new Label { Text = "Difficulty:", Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(diffLabel);

            difficultyComboBox = new ComboBox { Location = new Point(100, 20), DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
            difficultyComboBox.Items.AddRange(new string[] { "Easy", "Normal", "Hard" });
            difficultyComboBox.SelectedIndex = 1;
            this.Controls.Add(difficultyComboBox);

            // Timer label
            Label timeTextLabel = new Label { Text = "Time Left:", Location = new Point(220, 20), AutoSize = true };
            this.Controls.Add(timeTextLabel);

            timeLabel = new Label { Text = "0", Location = new Point(300, 20), AutoSize = true, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            this.Controls.Add(timeLabel);

            // Start button
            startButton = new Button { Text = "Start Quiz", Location = new Point(380, 15), Size = new Size(90, 30) };
            startButton.Click += startButton_Click;
            this.Controls.Add(startButton);

            // FlowLayoutPanel for quiz questions
            quizPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 60),
                Size = new Size(440, 300),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(quizPanel);

            // Previous/Next buttons
            prevButton = new Button { Text = "Previous", Location = new Point(50, 370), Size = new Size(100, 35) };
            prevButton.Click += PrevBtn_Click;
            this.Controls.Add(prevButton);

            nextButton = new Button { Text = "Next", Location = new Point(200, 370), Size = new Size(100, 35) };
            nextButton.Click += NextBtn_Click;
            this.Controls.Add(nextButton);

            prevButton.Visible = false;
            nextButton.Visible = false;

            // Timer
            timer1 = new System.Windows.Forms.Timer { Interval = 1000 };
            timer1.Tick += timer1_Tick;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            ConfigureQuiz();
            currentPage = 0;
            ShowPage(currentPage);

            prevButton.Visible = true;
            nextButton.Visible = true;

            startButton.Enabled = false;
            difficultyComboBox.Enabled = false;

            timer1.Start();
        }

        private void ConfigureQuiz()
        {
            string pagesInput = Interaction.InputBox("How many pages of math problems do you want?", "Quiz Configuration", "1");
            if (!int.TryParse(pagesInput, out totalPages) || totalPages < 1) totalPages = 1;

            string timeInput = Interaction.InputBox("Set total time for the quiz in seconds:", "Quiz Configuration", "120");
            if (!int.TryParse(timeInput, out totalTime) || totalTime < 5) totalTime = 120;

            timeLeft = totalTime;

            totalProblems = totalPages * problemsPerPage;

            operand1 = new int[totalProblems];
            operand2 = new int[totalProblems];
            operators = new string[totalProblems];
            answerBoxes = new NumericUpDown[totalProblems];
        }

        private void ShowPage(int pageIndex)
        {
            quizPanel.Controls.Clear();

            string difficulty = difficultyComboBox.SelectedItem.ToString();
            string[] possibleOps = difficulty == "Easy" ? new string[] { "+", "-" } : new string[] { "+", "-", "×", "÷" };

            int startIndex = pageIndex * problemsPerPage;
            int endIndex = Math.Min(startIndex + problemsPerPage, totalProblems);

            for (int i = startIndex; i < endIndex; i++)
            {
                if (operand1[i] == 0 && operand2[i] == 0)
                {
                    int x = randomizer.Next(1, 21);
                    int y = randomizer.Next(1, 21);
                    string op = possibleOps[randomizer.Next(possibleOps.Length)];
                    if (op == "÷") { y = randomizer.Next(1, 11); x = y * randomizer.Next(1, 11); }

                    operand1[i] = x;
                    operand2[i] = y;
                    operators[i] = op;
                }

                Panel problemRow = new Panel { Size = new Size(420, 50) };

                Label left = new Label { Text = operand1[i].ToString(), Location = new Point(0, 15), AutoSize = true };
                Label opLabel = new Label { Text = operators[i], Location = new Point(60, 15), AutoSize = true };
                Label right = new Label { Text = operand2[i].ToString(), Location = new Point(110, 15), AutoSize = true };
                Label equals = new Label { Text = "=", Location = new Point(160, 15), AutoSize = true };

                if (answerBoxes[i] == null)
                {
                    answerBoxes[i] = new NumericUpDown
                    {
                        Location = new Point(190, 12),
                        Width = 80,
                        Minimum = -1000,
                        Maximum = 1000,
                        Font = new Font("Segoe UI", 10),
                        BackColor = Color.White
                    };
                }

                problemRow.Controls.Add(left);
                problemRow.Controls.Add(opLabel);
                problemRow.Controls.Add(right);
                problemRow.Controls.Add(equals);
                problemRow.Controls.Add(answerBoxes[i]);

                quizPanel.Controls.Add(problemRow);
            }

            prevButton.Visible = pageIndex > 0;
            nextButton.Text = pageIndex == totalPages - 1 ? "Finish" : "Next";

            timeLabel.Text = timeLeft + " seconds";
            timeLabel.ForeColor = timeLeft <= 10 ? Color.Red : Color.Black;

            currentPage = pageIndex;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                timeLeft--;
                timeLabel.Text = timeLeft + " seconds";
                if (timeLeft <= 10) timeLabel.ForeColor = Color.Red;
            }
            else
            {
                timer1.Stop();
                EvaluateQuiz();
            }
        }

        private void NextBtn_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                ShowPage(currentPage);
            }
            else
            {
                EvaluateQuiz();
            }
        }

        private void PrevBtn_Click(object sender, EventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                ShowPage(currentPage);
            }
        }

        private void EvaluateQuiz()
        {
            timer1.Stop();
            int score = 0;
            string message = "Results:\n\n";

            for (int i = 0; i < totalProblems; i++)
            {
                int correctAnswer = 0;
                switch (operators[i])
                {
                    case "+": correctAnswer = operand1[i] + operand2[i]; break;
                    case "-": correctAnswer = operand1[i] - operand2[i]; break;
                    case "×": correctAnswer = operand1[i] * operand2[i]; break;
                    case "÷": correctAnswer = operand1[i] / operand2[i]; break;
                }

                bool correct = (answerBoxes[i].Value == correctAnswer);
                if (correct) score++;
                answerBoxes[i].BackColor = correct ? Color.LightGreen : Color.LightCoral;

                message += $"{operand1[i]} {operators[i]} {operand2[i]} = {answerBoxes[i].Value} " +
                           $"({correctAnswer}) → {(correct ? "Correct" : "Wrong")}\n";
            }

            message += $"\nScore: {score}/{totalProblems}";
            MessageBox.Show(message, "Quiz Results");

            startButton.Enabled = true;
            difficultyComboBox.Enabled = true;
            currentPage = 0;

            quizPanel.Controls.Clear();
            prevButton.Visible = false;
            nextButton.Visible = false;
        }
    }
}
