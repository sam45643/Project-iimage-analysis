using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Win32;



//namespace ImageAI
//{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
  //  public partial class MainWindow : Window
    //{
      //  public MainWindow()
        //{
          //  InitializeComponent();
        //}
    //}
//}


namespace ImageAI 
{
    public partial class MainWindow : Window
    {
        private static readonly string subscriptionKey = "1637d4fe3e5c4a37951392909a7a619d";
        private static readonly string endpoint = "https://sa3333.cognitiveservices.azure.com/";
        private ComputerVisionClient client;

        public MainWindow()
        {
            InitializeComponent();
            client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                Endpoint = endpoint
            };

            if (client == null)
            {
                MessageBox.Show("Failed to create Computer Vision client.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (dialog.ShowDialog() == true)
            {
                ImageUrlTextBox.Text = string.Empty;
                await LoadAndAnalyzeImage(dialog.FileName);
            }
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ImageUrlTextBox.Text))
            {
                await LoadAndAnalyzeImage(ImageUrlTextBox.Text);
            }
            else
            {
                MessageBox.Show("Please enter an image URL or select an image file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAndAnalyzeImage(string imageSource)
        {
            AnalyzeButton.IsEnabled = false;
            ResultsTextBox.Clear();

            try
            {
                if (Uri.IsWellFormedUriString(imageSource, UriKind.Absolute))
                {
                    ImageAnalysis analysis = await client.AnalyzeImageAsync(imageSource);
                    DisplayAnalysisResults(analysis);
                }
                else
                {
                    using (Stream stream = File.OpenRead(imageSource))
                    {
                        ImageAnalysis analysis = await client.AnalyzeImageInStreamAsync(stream);
                        DisplayAnalysisResults(analysis);
                        ImageControl.Source = new BitmapImage(new Uri(imageSource));
                    }
                }
            }
            catch (Exception ex)
            {
                ResultsTextBox.Text = $"Error analyzing image: {ex.Message}";
            }

            AnalyzeButton.IsEnabled = true;
        }

        private void DisplayAnalysisResults(ImageAnalysis analysis)
        {
            ResultsTextBox.Text += $"Tags:\n";
            foreach (var tag in analysis.Tags)
            {
                ResultsTextBox.Text += $"- {tag.Name} (Confidence: {tag.Confidence})\n";
            }

           // ResultsTextBox.Text += $"\nText:\n{string.Join("\n", analysis.Text)}\n";

            ResultsTextBox.Text += $"\nCaptions:\n- {analysis.Description.Captions[0].Text} (Confidence: {analysis.Description.Captions[0].Confidence})\n";

            ResultsTextBox.Text += $"\nObjects:\n";
            foreach (var obj in analysis.Objects)
            {
                ResultsTextBox.Text += $"- {obj.ObjectProperty} (Confidence: {obj.Confidence})\n";
            }

            ResultsTextBox.Text += $"\nPeople:\n";
            foreach (var person in analysis.Faces)
            {
                ResultsTextBox.Text += $"- Age: {person.Age}, Gender: {person.Gender} \n";
            }
        }
    }
}