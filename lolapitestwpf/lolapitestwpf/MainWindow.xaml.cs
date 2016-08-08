﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace lolapitestwpf
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private const string ApiKey = "?api_key=RGAPI-B1F0AC0F-8798-418B-B4A4-A2A9A1F1EAC2";
        private string _name = "";
        private string _region = "";
        private string _id = "";
        private readonly string[] _reglist = { "BR1", "EUN1", "EUW1", "JP1", "KR", "LA1", "LA2", "NA1", "OC1", "PBE1", "RU", "TR1" };

        private Dictionary<string, string> _summoner;
        private dynamic _currentGameInfo;

        private readonly Window _errorWindow1 = new ErrorWindow();

        private string DlBasicInfoJson(string region, string jsonadd1, string jsonadd2)
        {
            WebClient client = new WebClient();
            return client.DownloadString("https://" + region + jsonadd1 + region + jsonadd2 + _name + ApiKey);
        }

        private string DlCurrentGameJson(string jsonadd)
        {
            var http = (HttpWebRequest)WebRequest.Create("https://" + _region + jsonadd + _reglist[RegionPicker.SelectedIndex] + "/" + _id + ApiKey);
            HttpWebResponse response; //= null;
            try
            {
                 response = (HttpWebResponse)http.GetResponse();
            }
            catch (WebException we)
            {
                response = (HttpWebResponse)we.Response;
            }
            var status = response.StatusCode;
            if (status == HttpStatusCode.NotFound || status == HttpStatusCode.Forbidden)
            {
                _errorWindow1.Visibility = Visibility.Visible;
                return "404";
            }
            else
            {
                WebClient client = new WebClient();
                return client.DownloadString("https://" + _region + jsonadd + _reglist[RegionPicker.SelectedIndex] + "/" + _id + ApiKey);
            }
        }

        private void SetCurrentGameNotFound()
        {
            GameMap.Visibility = Visibility.Hidden;
            GameMod.Visibility = Visibility.Hidden;
            GameTyp.Visibility = Visibility.Hidden;
            GameNotFoundLbl.Visibility = Visibility.Visible;
        }

        private void GetBasicInfo()
        {
            SetSummoner();
            if (_region == "" || _name == "")
            {
                _errorWindow1.Visibility = Visibility.Visible;
            }
            else
            {
                var summonerInfo = DlBasicInfoJson(_region, ".api.pvp.net/api/lol/", "/v1.4/summoner/by-name/");
                summonerInfo = summonerInfo.Remove(0, 4 + _name.Length);
                summonerInfo = summonerInfo.Remove(summonerInfo.Length - 1);
                _summoner = JsonConvert.DeserializeObject<Dictionary<string, string>>(summonerInfo);
                _id = _summoner["id"];
            }
        }

        private void SetBasicInfo()
        {
            IdLbl.Content = "ID: " + _id;
            NameLbl.Content = "Name: " + _summoner["name"];
            LevelLbl.Content = "Level: " + _summoner["summonerLevel"];
            SummonerIcon.Source = (ImageSource)new ImageSourceConverter().ConvertFromString("http://ddragon.leagueoflegends.com/cdn/6.15.1/img/profileicon/" + _summoner["profileIconId"] + ".png");
        }

        private void GetCurrentInfo()
        {
            var currentGameInfoJ = DlCurrentGameJson(".api.pvp.net/observer-mode/rest/consumer/getSpectatorGameInfo/");
            if (currentGameInfoJ.Length > 3)
            {
                _currentGameInfo = JsonConvert.DeserializeObject(currentGameInfoJ);
            }
        }

        private void SetCurrentInfo()
        {
            GetCurrentInfo();
            if (_currentGameInfo == null)
                SetCurrentGameNotFound();
            else
            {
                GameMod.Content = _currentGameInfo.gameMode;
                GameTyp.Content = _currentGameInfo.gameType;
                var mapId = _currentGameInfo.mapId;
                GameMap.Content = mapId;
            }
        }

        private void UpdateUi()
        {
            SetBasicInfo();
        }

        private void SetSummoner()
        {
            _name = NameBox.Text;
            _region = RegionPicker.Text.ToLower();
        }

        private void DoMagic(object sender, RoutedEventArgs e)
        {
            GetBasicInfo();
            if (_errorWindow1.Visibility == Visibility.Collapsed)
            {
                UpdateUi();
            }
        }

        private void GetCurrentGame(object sender, RoutedEventArgs e)
        {
            if (_errorWindow1.Visibility == Visibility.Collapsed)
            {
                SetCurrentInfo();
            }
        }
    }
}