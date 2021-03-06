﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;
using ac_predictor.MongoDB;
using ac_predictor.AtCoder;

namespace ac_predictor.API
{
    public class APerfsController : ApiController
    {
        /// <summary>DBに入ってるコンテスト一覧の取得</summary>
        public JsonResult<string[]> Get()
        {
            APerfsDB db = new APerfsDB();
            List<string> contestIDs = db.ContestIDs;
            contestIDs.Reverse();
            return Json(contestIDs.ToArray());
        }

        /// <summary>APerf一覧の取得</summary>
        public JsonResult<Dictionary<string, double>> Get(string id)
        {
            APerfsDB db = new APerfsDB();
            APerfs aperfs = db.GetAPerfs(id);
            if (aperfs == null)
            {
                aperfs = new APerfs("", "", new Dictionary<string, double>());
            }
            return Json(aperfs.APerfDic);
        }

        public void Post(string contestID,string key)
        {
            if (ConfigurationManager.AppSettings["ApiKey"] != key) return;
            APerfsDB db = new APerfsDB();

            APerfs aPerfs = db.GetAPerfs(contestID);

            bool isContainContest = aPerfs != null;
            if (!isContainContest) aPerfs = new APerfs(contestID);

            double defaultValue = contestID.Substring(0, 3) == "abc" ? 800 : 1600;

            Standings standings = Standings.GetStandings(contestID);
            StandingData[] datas = standings.StandingsData;
            Dictionary<string, double> dict = aPerfs.APerfDic;
            int count = 0;
            foreach (var standing in datas)
            {
                if (!standing.IsRated) continue;
                if (dict.ContainsKey(standing.UserScreenName)) continue;
                CompetitionResult[] results = CompetitionResult.GetFromJson(standing.UserScreenName);
                double aperf = CompetitionResult.CalcAPerf(results, defaultValue);
                dict.Add(standing.UserScreenName, aperf);
                count++;
                if (count >= 100)
                {
                    Update();
                    count = 0;
                }
            }

            Update();
            void Update()
            {
                aPerfs.APerfDic = dict;
                if (!isContainContest) db.CreateAPerfs(aPerfs);
                else db.UpdateAPerfs(aPerfs);
            }
        }

        public void Delete(string contestID,string key)
        {
            if (ConfigurationManager.AppSettings["ApiKey"] != key) return;
            APerfsDB db = new APerfsDB();
            db.DeleteAPerfs(contestID);
        }
    }
}