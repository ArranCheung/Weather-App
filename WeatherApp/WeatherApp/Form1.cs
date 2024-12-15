using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WeatherApp
{
	public partial class Form1 : Form
	{
		weather Rq1 = new weather();
		public string weatherPropertiesSelection = "";
		public string weatherPollutentSelection = "";
		public string weatherWindSelection = "";

		public List<Series> PollutentGraphInfo = new List<Series>();
		public List<Series> WindGraphInfo = new List<Series>();
		public List<string> pollutentNames = new List<string> { "Particulate matter PM10", "Particulate matter PM2.5", "Carbon monoxide", "Carbon dioxide", "Nitrogen dioxide", "Sulpher dioxide" };
		List<string> windNames = new List<string> { "Wind (10m)", "Wind (80m)", "Wind (120m)", "Wind (180m)", "Wind gusts (10m)" };

		public Form1()
		{
			InitializeComponent();
		}

		public void changeDataDisplayed(string choice) // Updating the textboxes when the menu selection has been changed
		{
			string currentTime = DateTime.Now.ToString();
			currentTime = currentTime.Substring(currentTime.IndexOf(' ') + 1, 2);

			switch (choice)
			{
				case "Pollutent levels":
					// change the items in combo menu
					comboBox2.Visible = true;
					comboBox2.Items.Clear();
					comboBox2.Items.AddRange(pollutentNames.ToArray());

					string textBox = "";

					foreach (string item in Rq1.PollutentData.Keys)
					{
						textBox += (item + "\n");
						List<string> data = Rq1.PollutentData[item].Split(',').ToList();

						textBox += data[Convert.ToInt32(currentTime) - 1] + "\n\n";
					}

					richTextBox1.Text = textBox;


					// making the graph

					PollutentGraphInfo.Clear();
					foreach (string item in Rq1.PollutentData.Keys)
					{
						int counter = 0;
						Series DataSeries = new Series();

						for (int i = 0; i < 24; i++)
						{
							string data = Rq1.PollutentData[item].Split(',')[i];
							DataSeries.Points.AddXY(Rq1.Times[counter].Substring(Rq1.Times[counter].IndexOf('T') + 1, 5), Rq1.PollutentData[item].Split(',')[counter]);
							counter++;
						}
						PollutentGraphInfo.Add(DataSeries);
					}

					try
					{
						while (true)
						{
							chart1.Series.RemoveAt(0);
							chart1.ChartAreas.RemoveAt(0);
						}
					}
					catch { }

					if (weatherPollutentSelection != "")
					{
						int index = pollutentNames.IndexOf(weatherPollutentSelection);
						chart1.Series.Add(PollutentGraphInfo[index]);
						chart1.Series[0].Name = Rq1.PollutentData.Keys.ElementAt(index);
						chart1.ChartAreas.Add(new ChartArea());
					}
					else
					{
						chart1.Series.Add(PollutentGraphInfo[0]);
						chart1.Series[0].Name = Rq1.PollutentData.Keys.ElementAt(0);
						chart1.ChartAreas.Add(new ChartArea());
					}

					break;


				case "Temperature":
					comboBox1.SelectedItem = "Temperature";

					Rq1.temperature();

					string weatherText = $"Time\t\t\tTemperature\n";
					Series series = new Series();
					series.ChartType = SeriesChartType.Line;
					series.Name = Rq1.Place;

					for (int i = 0; i < 24; i++)
					{
						string time = Rq1.timeStamps.ElementAt(i).Key.Substring(Rq1.timeStamps.ElementAt(i).Key.IndexOf('T') + 1, 5);
						float temp = Rq1.timeStamps.ElementAt(i).Value;

						weatherText += $"{time}\t\t\t{temp}\n";

						// creating chart
						series.Points.AddXY(time, temp);
					}
					try
					{
						chart1.Series.RemoveAt(0);
						chart1.ChartAreas.RemoveAt(0);
					}
					catch { }
					chart1.Series.Add(series);
					chart1.ChartAreas.Add(new ChartArea());

					richTextBox1.Text = weatherText;

					break;


				case "Wind speed":
					// chaning items in combo menu
					comboBox2.Visible = true;
					comboBox2.Items.Clear();
					comboBox2.Items.AddRange(windNames.ToArray());

					// updating the textbox
					textBox = "";

					for (int i = 0; i < Rq1.WindData.Keys.Count; i++)
					{
						List<string> list = new List<string> { "Wind speed:", "Wind direction:" };

						string type = Rq1.WindData.Keys.ElementAt(i);
						string[] data = Rq1.WindData[type];

						textBox += type + "\n";

						foreach (string item in data)
						{
							textBox += ($"{list[0]} {item.Split(',')[Convert.ToInt32(currentTime)]}\n");
							list.RemoveAt(0);
						}

						textBox += "\n";
					}
					richTextBox1.Text = textBox;


					// updating the graph

					foreach (string[] Wind in Rq1.WindData.Values)
					{
						foreach (string WindProperty in Wind)
						{
							string[] sp = WindProperty.Split(',');
							Series WindSeries = new Series();

							for (int i = 0; i < 24; i++)
							{
								string time = Rq1.timeStamps.ElementAt(i).Key.Substring(Rq1.timeStamps.ElementAt(i).Key.IndexOf('T') + 1, 5);
								string data = sp[i];

								WindSeries.Points.AddXY(time, data);
							}
							WindGraphInfo.Add(WindSeries);
						}
					}

					try
					{
						while (true)
						{
							chart1.Series.RemoveAt(0);
							chart1.ChartAreas.RemoveAt(0);
						}
					}
					catch { }

					if (weatherWindSelection != "")
					{
						int index = windNames.IndexOf(weatherWindSelection);
						chart1.Series.Add(WindGraphInfo[index * 2]);
						chart1.Series[0].Name = windNames[index];
						chart1.ChartAreas.Add(new ChartArea());
						comboBox2.SelectedItem = windNames[index];
					}
					else
					{
						chart1.Series.Add(WindGraphInfo[0]);
						chart1.Series[0].Name = windNames[0];
						chart1.ChartAreas.Add(new ChartArea());
						comboBox2.SelectedIndex = 0;
					}

					break;


				case "Rain probability":
					break;
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void button1_Click(object sender, EventArgs e) // when the search button is clicked
		{
			string text = search.Text;
			panel2.Visible = true;

			// call api and get info
			richTextBox1.Text = text;
			Rq1.Place = text;
			bool valid = Rq1.start();

			if (valid)
			{
				if (weatherPropertiesSelection != "")
				{
					Rq1.changeType(weatherPropertiesSelection);
					changeDataDisplayed(weatherPropertiesSelection);
				}
				else
				{
					Rq1.changeType("Temperature");
					changeDataDisplayed("Temperature");
				}
			}
			else
			{
				richTextBox1.Text = "Location too specific";
			}

		}

		private void search_TextChanged(object sender, EventArgs e)
		{

		}

		private void panel2_Paint(object sender, PaintEventArgs e)
		{

		}

		private void richTextBox1_TextChanged(object sender, EventArgs e)
		{

		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{

		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) // when the selection box is changed
		{
			comboBox2.Visible = false;
			comboBox2.SelectedItem = null;

			string choice = comboBox1.SelectedItem.ToString();

			Rq1.changeType(choice);
			changeDataDisplayed(choice);
		}

		private void chart1_Click(object sender, EventArgs e)
		{

		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBox2.SelectedItem == null) return;  
			string choice = comboBox2.SelectedItem.ToString();

			// clearing the chart out
			try
			{
				while (true)
				{
					chart1.Series.RemoveAt(0);
					chart1.ChartAreas.RemoveAt(0);
				}
			}
			catch { }

			try
			{
				// adding sources to the chart
				if (comboBox1.SelectedItem == "Pollutent levels")
				{
					int index = pollutentNames.IndexOf(choice);
					weatherPollutentSelection = choice;
					chart1.Series.Add(PollutentGraphInfo[index]);
				}
				else if (comboBox1.SelectedItem == "Wind speed")
				{
					int index = windNames.IndexOf(choice);
					weatherWindSelection = choice;
					chart1.Series.Add(WindGraphInfo[index]);
				}
				
				chart1.ChartAreas.Add(new ChartArea());
				chart1.Series[0].Name = choice;
			}
			catch { }
		}
	}

	public class weather
	{
		public string Place { get; set; }
		public weather(string city)
		{
			Place = city;
		}
		public weather()
		{
			Place = "";
		}

		public string longitude { get; set; }
		public string latitude { get; set; }
		public string elevation { get; set; }
		public string population { get; set; }
		public string timezone { get; set; }
		public string country { get; set; }
		public Dictionary<string, float> timeStamps { get; set; }
		public Dictionary<string, string> PollutentData { get; set; }
		public Dictionary<string, string[]> WindData { get; set; }
		public Dictionary<string,string> RaindData { get; set; }
		public List<string> Times { get; set; }

		public void changeType(string type) // gets the relavent information and subroutines required for that information to be fetched
		{
			switch (type)
			{
				case "Temperature":
					temperature();
					break;
				// make more types for pollen and wind speed etc
				case "Pollutent levels":
					pollutents();
					break;
				case "Rain probability":
					rain();
					break;
				case "Wind speed":
					wind();
					break;
			}
			Debug.WriteLine("type change");
		}
		public string makeRq(string path) // formulates a request to the api
		{
			//formatting and making request
			WebRequest request = WebRequest.Create(path);
			request.Method = WebRequestMethods.Http.Get;
			request.ContentType = "application/json; charset=utf-8";
			string text;
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			// reading info from file
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				text = reader.ReadToEnd();
			}

			return text;
		}

		public bool start() // gets the relevant information about the searched place
		{

			string text = makeRq($"https://geocoding-api.open-meteo.com/v1/search?name={Place}&count=1&language=en&format=json");

			//formatting text
			try
			{
				text = text.Substring(text.IndexOf('['), text.Length - text.IndexOf('['));
				text = text.Substring(2, text.Length - (text.Length - text.IndexOf(']')) - 3);

				string[] info = text.Split(',');
				Dictionary<string, string> KeyInfo = new Dictionary<string, string>();

				foreach (string s in info)
				{
					string final = "";
					foreach (char c in s)
					{
						if (c != '"') final += c;
					}
					if (final.StartsWith("postcodes") || !final.Contains(":")) continue;

					string key = final.Split(':')[0];
					string value = final.Split(':')[1];

					KeyInfo.Add(key, value);

				}

				// storing info in class
				longitude = KeyInfo["longitude"];
				latitude = KeyInfo["latitude"];
				elevation = KeyInfo["elevation"];
				timezone = KeyInfo["timezone"];
				try
				{
					population = KeyInfo["population"];
				}
				catch { }

				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
				return false;
			}

		}

		public bool temperature() // gets the temperature
		{
			try
			{
				// creating weather request
				string weather = makeRq($"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&hourly=temperature_2m");

				// formatting text
				string originaltime = weather.Substring(weather.IndexOf('['), weather.Length - weather.IndexOf('['));
				string times = originaltime.Substring(2, originaltime.Length - (originaltime.Length - originaltime.IndexOf(']')) - 3);

				string temps = weather.Replace(times, string.Empty);
				string[] sp = temps.Split(':');
				temps = sp.Last();
				temps = temps.Substring(1, temps.Length - 4);

				// creating lists
				string[] Times = times.Split(',');
				string[] Temperature = temps.Split(',');


				// adding the times to a dictionary with their temperatures
				Dictionary<string, float> TimeTemps = new Dictionary<string, float>();

				for (int i = 0; i < Temperature.Length; i++)
				{
					TimeTemps.Add(Times[i], float.Parse(Temperature[i]));
				}
				timeStamps = TimeTemps;
				return true;
			}
			catch (ArgumentOutOfRangeException) { return false; }
		}
		public void wind()
		{
			try
			{
				WindData = new Dictionary<string, string[]>();

				string weather = makeRq($"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&hourly=wind_speed_10m,wind_speed_80m,wind_speed_120m,wind_speed_180m,wind_direction_10m,wind_direction_80m,wind_direction_120m,wind_direction_180m,wind_gusts_10m");

				int start = weather.LastIndexOf("wind_speed_10m");
				weather = weather.Substring(start);

				List<string> windData = new List<string>();
				List<string> windNames = new List<string> { "Wind (10m)", "Wind (80m)", "Wind (120m)", "Wind (180m)", "Wind gusts (10m)" };

				while (true)
				{
					try
					{
						bool inside = false; string dataToAdd = "";

						foreach (char item in weather)
						{
							if (item == '[' || item == ']')
							{
								if (inside)
								{
									inside = false;
									windData.Add(dataToAdd);
									dataToAdd = "";
								}
								else { inside = true; }
								continue;
							}
							if (inside)
							{
								dataToAdd += item;
							}
						}
						break;
					}
					catch
					{
						break;
					}
				}

				for (int i = 0; i < 4; i++)
				{
					WindData.Add(windNames[i], new string[] { windData[i], windData[i + 4] });
				}
				WindData.Add(windNames[4], new string[] { windData[4] });
			}
			catch { }
		}
		public void rain()
		{
			try
			{
				RaindData = new Dictionary<string, string>();

				string weather = makeRq($"");
			}
			catch { }
		}
		public void pollutents() // stores the pollutents
		{
			try
			{
				PollutentData = new Dictionary<string, string>();

				string weather = makeRq($"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={latitude}&longitude={longitude}&hourly=pm10,pm2_5,carbon_monoxide,carbon_dioxide,nitrogen_dioxide,sulphur_dioxide,ozone");

				string info = weather.Substring(weather.Substring(1, weather.Length - 2).IndexOf('{'), weather.Length - 2 - weather.Substring(1, weather.Length - 2).IndexOf('{'));
				string units = info.Substring(2, info.IndexOf('}') - 3);

				// times
				string data = info.Substring(info.IndexOf('}') + 11, info.Length - info.IndexOf('}') - 11);
				string hour = data.Substring(data.IndexOf('[') + 1, data.IndexOf(']') - data.IndexOf('[') - 1);

				Times = hour.Split(',').ToList();

				List<string> pollutentData = new List<string>();
				List<string> pollutentNames = new List<string> { "Particulate matter PM10", "Particulate matter PM2.5", "Carbon monoxide", "Carbon dioxide", "Nitrogen dioxide", "Sulpher dioxide" };

				string add = data.Substring(data.IndexOf('[') + 1, data.Length - data.IndexOf('[') - 3);
				add = add.Substring(add.IndexOf(']') + 1, add.Length - add.IndexOf(']') - 1);

				File.WriteAllText("output.txt", add);

				while (true)
				{
					try
					{
						bool inside = false; string dataToAdd = "";

						foreach (char item in add)
						{
							if (item == '[' || item == ']')
							{
								if (inside)
								{
									inside = false;
									pollutentData.Add(dataToAdd);
									dataToAdd = "";
								}
								else { inside = true; }
								continue;
							}
							if (inside)
							{
								dataToAdd += item;
							}
						}
						break;
					}
					catch
					{
						break;
					}
				}

				for (int i = 0; i < pollutentData.Count; i++)
				{
					PollutentData[pollutentNames[i]] = pollutentData[i];
				}

			}
			catch (ArgumentOutOfRangeException) { }
		}
	}

}
