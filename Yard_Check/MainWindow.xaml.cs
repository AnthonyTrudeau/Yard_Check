using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using Spire.Xls;

namespace Yard_Check
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Variables
        string todaysDate = DateTime.Now.ToString("MM-dd-yyyy");
        string selectedArea = "";
        string trailerNumber = "";
        string path;

        private static string[] yardCheckAreasArray = { "Back Fourty", "Perimiter", "West Pad", "East Pad" };

        public MainWindow()
        {
            InitializeComponent();
        }

        //Method thats run when the page loads the first time
        private void YardCheckWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Save file locations
            string internalRoot = @"C:\Temp";
            string internalSubDir = @"C:\Temp\YardCheck";

            //Sort out the array
            Array.Sort(yardCheckAreasArray);

            //Set the combo box
            area_combo_box.ItemsSource = yardCheckAreasArray;

            //Set the default radio to empty
            radio_empty.IsChecked = true;

            //Check if the folders exists
            if (!Directory.Exists(internalRoot))
            {
                Directory.CreateDirectory(internalRoot);
            }
            if (!Directory.Exists(internalSubDir))
            {
                Directory.CreateDirectory(internalSubDir);
            }
        }

        //Drag Bar
        private void drag_bar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Allow the user to drag the window when they hold the left mouse on the top bar
            DragMove();
        }

        //Disallow the user to maximize (Only other way i found was to use a api that also dissabled pining it to the sides)
        //I wanted the GUI to be able to be pinned to the sides
        private void YardCheckWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }

        //Code for the close menu
        private void button_close_app_Click(object sender, RoutedEventArgs e)
        {

            //Turn off the close button so you can not
            //open multiple instances of the close menu
            //Slightly useless
            button_close_app.IsEnabled = false;

            //Start up an instance of the close menu
            var CloseMenu = new PopupWindow();

            //Variables for the Close Window starting location getting the screen size and the menu size
            double closeMenuTop = ((System.Windows.SystemParameters.PrimaryScreenHeight / 2) - (CloseMenu.Height / 2));
            double closeMenuLeft = ((System.Windows.SystemParameters.PrimaryScreenWidth / 2) - (CloseMenu.Width / 2));

            //Center the close menu on set location
            CloseMenu.Top = closeMenuTop;
            CloseMenu.Left = closeMenuLeft;

            //Open the popup and retirieve the result
            bool? TestClose = CloseMenu.ShowDialog();

            //Check if the return was true
            if (TestClose == true)
            {
                //Close the entire app
                Close();
            }

            //Turn the close button back on
            button_close_app.IsEnabled = true;
        }

        //The area drop bar
        private void button_change_area_Click(object sender, RoutedEventArgs e)
        {
            //Change the color of the "Smart user guide"
            smart_users_label_trailerbox.Foreground = Brushes.Transparent;

            //Reinable the area combo box
            area_combo_box.IsEnabled = true;

            //Turn off the entry box until an are is choosen
            grid_entry_text_box.IsEnabled = false;

            //turn off the change area button
            button_change_area.IsEnabled = false;

            //Turn off the end yard check button
            button_end_yc.IsEnabled = false;

            //Clear the display
            trailers_display_box.Text = "Trailer Numbers Will Appear Here";
        }

        //Check to make sure an area is selected
        private void area_combo_box_DropDownClosed(object sender, EventArgs e)
        {
            //Check the combo box to see if an item is selected
            if (area_combo_box.SelectedIndex != -1)
            {
                //Turn on the Start yard check button
                button_start_yc.IsEnabled = true;
            }
        }

        //Button that starts the yard check
        private void button_start_yc_Click(object sender, RoutedEventArgs e)
        {
            //Turn off the area combo box
            area_combo_box.IsEnabled = false;
            selectedArea = area_combo_box.SelectedItem.ToString();

            //Need to set up the save file for the selected area
            path = @"C:\Temp\YardCheck\" + selectedArea + "_" + todaysDate + ".txt";

            //Turn off the Start button
            button_start_yc.IsEnabled = false;

            //Turn on the Change Area button
            button_change_area.IsEnabled = true;

            //Turn on the Keypad
            grid_entry_text_box.IsEnabled = true;

            //Turn on the end yard check button
            button_end_yc.IsEnabled = true;

            //Change the color of the "Smart user guide"
            smart_users_label_trailerbox.Foreground = Brushes.White;

            //Needs to check to see if there is already a version running of the selected yard check and open it if its there,
            //or it needs to start  a new yard check with a time stamp when the button is pressed
            if (!File.Exists(path))
            {
                //Create the txt file
                using (StreamWriter sw = File.CreateText(path))
                {
                    //Line 0 should be the start time
                    string startTime = DateTime.Now.ToString("HH:mm") + "-START TIME";
                    
                    sw.WriteLine(startTime);
                }
            }

            //Update the display
            UpdateTrailerDisplay();
        }

        //Button that ends the yard check
        private void button_end_yc_Click(object sender, RoutedEventArgs e)
        {

            //Change the Delete button to say it is loading
            string stopText = button_end_yc.Content.ToString();
            button_end_yc.Content = "LOADING...";
            button_end_yc.IsEnabled = false;

            //Read the file file should exsist at this point
            string[] readData = System.IO.File.ReadAllLines(path);
            string endTime = DateTime.Now.ToString("HH:mm") + "-END TIME";

            string[] timeCheck = readData[readData.Length - 1].Split("-".ToCharArray());

            //Check for end time tag if not there add it in
            if (!timeCheck[1].Equals("END TIME"))
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(endTime);

                }
            }
            UpdateTrailerDisplay();
            //Pull data again
            readData = System.IO.File.ReadAllLines(path);
            string startTime = readData[0];
            endTime = readData[readData.Length - 1];

            //Using spire excel free 5 page maximim of 200 lines

            //For saving to temp
            string xcelPath = path.Replace(".txt", ".xls");

            //set up the path for the template
            string dataPath = AppDomain.CurrentDomain.BaseDirectory; 
            string xcelToCopy = dataPath + (@"..\..\YardCheckData\TestCopy.xlsx");

            //Loading the template book workbook object
            Workbook templateBook = new Workbook();
            templateBook.LoadFromFile(xcelToCopy);
            //Set up the template sheet
            Worksheet templateSheet = templateBook.Worksheets[0];

            //Insert the Start and End times
            string[] startTimeTrimed = readData[0].Split("-".ToCharArray());
            templateSheet.Range["L2"].Text = startTimeTrimed[0];
            string[] endTimeTrimed = readData[readData.Length - 1].Split("-".ToCharArray());
            templateSheet.Range["P2"].Text = endTimeTrimed[0];
            //Insert Dates
            templateSheet.Range["V1"].Text = todaysDate;
            templateSheet.Range["Z49"].Text = "Date:\n" + todaysDate;
            templateSheet.Range["Z54"].Text = "Date:\n" + todaysDate;

            templateSheet.Range["Z51"].Text = "Time:\n" + endTimeTrimed[0];
            templateSheet.Range["Z56"].Text = "Time:\n" + endTimeTrimed[0];



            //Set up the new version of the workbook
            Workbook yardBook = new Workbook();

            string tRow = "B";
            int page = 0;
            int row = 0;
            int column = 0;
            int columnMaxLength = 58;
            int pageLimit = 5; //5 page limit from Spire



            foreach (string trailerInput in readData)
            {
                if (page < pageLimit) //5 page limit from Spire
                {
                    //Split every entry in the array
                    string[] trailer = trailerInput.Split("-".ToCharArray());
                    //Weed out start and end times
                    if (!trailer[1].Equals("END TIME") && !trailer[1].Equals("START TIME"))
                    {
                        //Run up to the max length
                        if (column <= columnMaxLength)
                        {
                            //Print the data to the sheet
                            templateSheet.Range[tRow + (5 + column).ToString()].Text = trailer[0];
                            if (row == 0)
                            {
                                if (trailer[1].Equals("PALLETS"))
                                {
                                    templateSheet.Range["G" + (5 + column).ToString()].Text = "X";
                                }
                                else if (trailer[1].Equals("EMPTY"))
                                {
                                    templateSheet.Range["H" + (5 + column).ToString()].Text = "X";
                                }
                                else if (trailer[1].Equals("VOLUME"))
                                {
                                    templateSheet.Range["I" + (5 + column).ToString()].Text = "X";
                                }
                                else if (trailer[1].Equals("?????"))
                                {
                                    //Here for fun we dont actually use the Seal spot
                                }
                            }
                            else if (row  == 1)
                            {
                                if (trailer[1].Equals("PALLETS"))
                                {
                                    templateSheet.Range["V" + (5 + column).ToString()].Text = "X";
                                }
                                else if (trailer[1].Equals("EMPTY"))
                                {
                                    templateSheet.Range["W" + (5 + column).ToString()].Text = "X";
                                }
                                else if (trailer[1].Equals("VOLUME"))
                                {
                                    templateSheet.Range["X" + (5 + column).ToString()].Text = "X";
                                }
                                else if (trailer[1].Equals("?????"))
                                {
                                    //Here for fun we dont actually use the Seal spot
                                }
                            }
                            column++;
                            //Check to see if the limit is now hit
                            if (column >= columnMaxLength)
                            {
                                //If it is hit make a new row
                                column = 0;
                                row++;
                                columnMaxLength = 0;
                                //Check if at the row limit
                                if (row >= 2)
                                {
                                    //If it is at the limit it starts a new page, Spire does not let you have a template to run with.
                                    //From the notes i read
                                    row = 0;
                                    yardBook.Worksheets.AddCopy(templateSheet);
                                    page++;
                                    columnMaxLength = 58;
                                    column = 0;
                                    for (int counterColumn1 = 0; counterColumn1 <= columnMaxLength; counterColumn1++)
                                    {
                                        templateSheet.Range["B" + (5 + counterColumn1).ToString()].Text = "";
                                        templateSheet.Range["G" + (5 + counterColumn1).ToString()].Text = "";
                                        templateSheet.Range["H" + (5 + counterColumn1).ToString()].Text = "";
                                        templateSheet.Range["I" + (5 + counterColumn1).ToString()].Text = "";
                                        templateSheet.Range["J" + (5 + counterColumn1).ToString()].Text = "";
                                    }
                                    for (int counterColumn2 = 0; counterColumn2 <= columnMaxLength; counterColumn2++)
                                    {
                                        templateSheet.Range["Q" + (5 + counterColumn2).ToString()].Text = "";
                                        templateSheet.Range["V" + (5 + counterColumn2).ToString()].Text = "";
                                        templateSheet.Range["W" + (5 + counterColumn2).ToString()].Text = "";
                                        templateSheet.Range["X" + (5 + counterColumn2).ToString()].Text = "";
                                        templateSheet.Range["Y" + (5 + counterColumn2).ToString()].Text = "";
                                    }
                                }
                                if (row == 0)
                                {
                                    tRow = "B";
                                }
                                else if (row == 1)
                                {
                                    tRow = "Q";
                                    columnMaxLength = 37;
                                }
                            }
                        }
                    }
                }
            }

            //Create the last sheet needed
            yardBook.Worksheets.AddCopy(templateSheet);
            //Remove the unneeded sheets

            yardBook.Worksheets.Remove("Sheet1");
            yardBook.Worksheets.Remove("Sheet2");
            yardBook.Worksheets.Remove("Sheet3");

            try
            {
                //Start a filestream
                FileStream file_stream = new FileStream(xcelPath, FileMode.Create);
                yardBook.SaveToStream(file_stream);

                //Open Excel Document
                System.Diagnostics.Process.Start(xcelPath);
            }
            catch { MessageBox.Show("Excel document already open close and try again.", "Error"); }

            //Turn the button back on
            button_end_yc.Content = stopText;
            button_end_yc.IsEnabled = false;
        }

        //Change the contents of the trailer text box depending on if its enabled
        private void trailer_text_box_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (trailer_text_box.IsEnabled == true)
            {
                trailer_text_box.Text = "";
            }
            else
            {
                trailer_text_box.Text = "Enter Trailer Number";
            }

        }

        //Take the button inputs
        private void button_type(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)e.Source;

            //Insert the value at the end of the string
            trailerNumber = trailerNumber.Insert(trailerNumber.Length, btn.Content.ToString());


            //Post the trailer number to the display
            trailer_text_box.Text = trailerNumber;
        }

        private void button_delete_Click(object sender, RoutedEventArgs e)
        {
            //Only delete if there is somthing to delete
            if (trailerNumber.Length > 0)
            {
                trailerNumber = trailerNumber.Remove(trailerNumber.Length - 1, 1);

                //Post the trailer number to the display
                trailer_text_box.Text = trailerNumber;
            }
        }

        //Deals with if you type somthing in on your own
        private void trailer_text_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Update the trailer number
            trailerNumber = trailer_text_box.Text;
        }

        private void button_enter_trailer_Click(object sender, RoutedEventArgs e)
        {
            EnterTrailer();
        }
        private void Enter_Pressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EnterTrailer();
            }
        }

        private void EnterTrailer()
        {
            //Pull in the current number
            string enteredTrailerNumber = trailerNumber.Replace("-", "");
            enteredTrailerNumber = enteredTrailerNumber.ToUpper();
            string trailerStatus = "";
            bool existsinArray = false;

            if (enteredTrailerNumber.Length > 0)
            {
                //Get the trailers status
                if (radio_empty.IsChecked == true)
                {
                    trailerStatus = "EMPTY";
                }
                else if (radio_pallets.IsChecked == true)
                {
                    trailerStatus = "PALLETS";
                }
                else if (radio_question.IsChecked == true)
                {
                    trailerStatus = "?????";
                }
                else if (radio_volume.IsChecked == true)
                {
                    trailerStatus = "VOLUME";
                    //Add a screen asking for a note?
                }

                //The file should exsist at this point already
                if (!File.Exists(path))
                {
                    //Create the txt file
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        //Line 0 should be the start time
                        string startTime = DateTime.Now.ToString("HH:mm");
                        sw.WriteLine(startTime);
                    }
                }

                //Read the file and check to see if the number exists already
                string[] readData = System.IO.File.ReadAllLines(path);

                int counter = 0;
                foreach (string element in readData)
                {
                    if (element.Contains("-"))
                    {
                        string[] temp = element.Split("-".ToCharArray());

                        if (temp[0].Equals(enteredTrailerNumber))
                        {
                            existsinArray = true;
                            if (!temp[1].Equals(trailerStatus))
                            {
                                readData[counter] = (enteredTrailerNumber + "-" + trailerStatus);

                                File.WriteAllLines(path, readData);

                            }
                        }
                    }
                    counter++;
                }

                //If the number is not in the array
                if (existsinArray == false)
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(enteredTrailerNumber + "-" + trailerStatus);
                    }
                }


                //Delete the old numbers
                trailerNumber = "";
                trailer_text_box.Text = trailerNumber;

                //Update the display
                UpdateTrailerDisplay();
            }
        }

        private void UpdateTrailerDisplay()
        {
            //Read the file
            string[] readData = System.IO.File.ReadAllLines(path);

            trailers_display_box.Text = "";

            foreach (string element in readData)
            {
                trailers_display_box.AppendText(element + "\n");
            }
        }
    }
}