using System;
using System.Collections.Generic;
using System.Text;
using ParseServiceNC2.DB;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ParseServiceNC2
{
    static class DatabaseClass
    {
        static private List<Operations> operations = new List<Operations>();
        static private List<Splavy> splavy = new List<Splavy>();
        static private List < Ukazaniya > ukazaniya = new List<Ukazaniya>();
        static private List<Contracts> contracts = new List<Contracts>();
        static public bool GetLastStatus(out List<PechStatus> lastStatus)
        {
            lastStatus = new List<PechStatus>();
            try
            {
                using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
                {
                    var innerQuery = (from a in db.PechStatus
                                      group a by a.Pech into b
                                      select b.Max(x => x.ProcessId)).ToList();

                    var result = (from a in db.PechStatus
                                  where innerQuery.Contains(a.ProcessId)
                                  orderby a.Pech
                                  select a).Include(a => a.Operation).Include(a => a.Contract)
                                  .Include(a => a.Splav).Include(a => a.Ukazanie).ToList();
                    lastStatus = result;
                }
            }
            catch(Exception e)
            {
                LogClass.CreateErrorLog("Can't get last status from DataBase: " + e.Message);
                return false;
            }
            return true;
        }
        static public bool UdateStatus(List<PechStatus> stats)
        {
            try
            {
                List<int> statID = new List<int>();
                foreach (var s in stats)
                    statID.Add(s.ProcessId);
                using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
                {
                    var result = (from a in db.PechStatus
                                  where statID.Contains(a.ProcessId)
                                  orderby a.ProcessId
                                  select a);
                    foreach (var r in result)
                    {
                        var s = stats.Find(x => x.ProcessId == r.ProcessId);
                        if (s.SplavId != r.SplavId || s.UkazanieId != r.UkazanieId || s.ContractId != s.ContractId)
                        {
                            db.PechStatus.Add(s);
                            continue;
                        }
                        r.EndTeor = s.EndTeor.HasValue ? r.EndTeor.HasValue ?
                            r.EndTeor > s.EndTeor.Value.AddMinutes(1) || r.EndTeor < s.EndTeor.Value.AddMinutes(-1) ?
                            s.EndTeor : r.EndTeor : s.EndTeor : r.EndTeor;
                        r.EndFact = s.EndFact;
                    }
                    db.SaveChanges();
                }
            }
            catch
            {
                LogClass.CreateErrorLog("Can't update status from DataBase.");
                return false;
            }
            return true;
        }
        static public bool AddStatus(List<PechStatus> stats)
        {
            try
            {
                using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
                {
                    foreach (var s in stats)
                        db.PechStatus.Add(s);
                    db.SaveChanges();
                }
            }
            catch
            {
                LogClass.CreateErrorLog("Can't update status from DataBase.");
                return false;
            }
            return true;
        }
        static public List<PechStatus> FormatStatus(List<PechStatus> oldStatus)
        {
            try
            {
                //Correct Operation
                List<string> param = new List<string>();
                foreach (var s in oldStatus)
                    if (string.IsNullOrEmpty(param.Find(x => x == s.Operation.Operation)))
                        param.Add(s.Operation.Operation);
                bool flag = false;
                using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
                {
                    foreach (var s in param)
                        if (operations.Find(x => x.Operation == s).Operation == "")
                        {
                            db.Operations.Add(new Operations(s));
                            flag = true;
                        }
                    if (flag)
                        db.SaveChanges();
                }
                if (flag)
                    UpdateOperation();
                foreach (var s in oldStatus)
                {
                    s.OperationId = operations.Find(x => x.Operation == s.Operation.Operation).OperationId;
                    s.Operation = null;
                }

                //Correct Splavy
                param = new List<string>();
                foreach (var s in oldStatus)
                {
                    if (s.Splav == null)
                        continue;
                    if (string.IsNullOrEmpty(param.Find(x => x == s.Splav.Splav)))
                        param.Add(s.Splav.Splav);
                }

                flag = false;
                foreach (var s in param)
                    flag |= splavy.Find(x => x.Splav == s) == null;
                if (flag)
                    UpdateSplav(param);

                flag = false;
                using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
                {
                    foreach (var s in param)
                        if (splavy.Find(x => x.Splav == s) == null)
                        {
                            db.Splavy.Add(new Splavy(s));
                            flag = true;
                        }
                    if (flag)
                        db.SaveChanges();
                }
                if (flag)
                    UpdateSplav(param);
                foreach (var s in oldStatus)
                {
                    if (s.Splav == null)
                        continue;
                    s.SplavId = splavy.Find(x => x.Splav == s.Splav.Splav).SplavId;
                    s.Splav = null;
                }

                //Correct Contract
                param = new List<string>();
                foreach (var s in oldStatus)
                {
                    if (s.Contract == null)
                        continue;
                    if (string.IsNullOrEmpty(param.Find(x => x == s.Contract.Contract)))
                        param.Add(s.Contract.Contract);
                }

                flag = false;
                foreach (var s in param)
                    flag |= contracts.Find(x => x.Contract == s) == null;
                if (flag)
                    UpdateContract(param);

                flag = false;
                using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
                {
                    foreach (var s in param)
                        if (contracts.Find(x => x.Contract == s) == null)
                        {
                            db.Contracts.Add(new Contracts(s));
                            flag = true;
                        }
                    if (flag)
                        db.SaveChanges();
                }
                if (flag)
                    UpdateContract(param);
                foreach (var s in oldStatus)
                {
                    if (s.Contract == null)
                        continue;
                    s.ContractId = contracts.Find(x => x.Contract == s.Contract.Contract).ContractId;
                    s.Contract = null;
                }

                //Correct Ukazanie
                param = new List<string>();
                foreach (var s in oldStatus)
                {
                    if (s.Ukazanie == null)
                        continue;
                    if (string.IsNullOrEmpty(param.Find(x => x == s.Ukazanie.Ukazanie)))
                        param.Add(s.Ukazanie.Ukazanie);
                }

                flag = false;
                foreach (var s in param)
                    flag |= ukazaniya.Find(x => x.Ukazanie == s) == null;
                if (flag)
                    UpdateUkazanie(param);

                flag = false;
                using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
                {
                    foreach (var s in param)
                        if (ukazaniya.Find(x => x.Ukazanie == s) == null)
                        {
                            db.Ukazaniya.Add(new Ukazaniya(s));
                            flag = true;
                        }
                    if (flag)
                        db.SaveChanges();
                }
                if (flag)
                    UpdateUkazanie(param);
                foreach (var s in oldStatus)
                {
                    if (s.Ukazanie == null)
                        continue;
                    s.UkazanieId = ukazaniya.Find(x => x.Ukazanie == s.Ukazanie.Ukazanie).UkazanieId;
                    s.Ukazanie = null;
                }
            }
            catch(Exception e)
            {
                LogClass.CreateErrorLog("Can't formattin status list: " + e.Message);
                return new List<PechStatus>();
            }
            return oldStatus;
        }
        static public bool TestDataBase()
        {
            try
            {
                UpdateOperation();
            }
            catch (Exception e)
            {
                LogClass.CreateErrorLog("DataBase Error: " + e.Message);
                LogClass.CreateErrorLog("Connection string: " + ConfigClass.ConnectionString);
                return false;
            }
            return true;
        }
        static private void UpdateOperation()
        {
            using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
                {
                    operations = (from a in db.Operations
                                  orderby a.OperationId
                                  select a).ToList();
                }
        }
        static private void UpdateSplav(List<string> s)
        {
            using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
            {
                splavy = (from a in db.Splavy
                          where s.Contains(a.Splav)
                          orderby a.SplavId
                          select a).ToList();
            }
        }
        static private void UpdateContract(List<string> s)
        {
            using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
            {
                contracts = (from a in db.Contracts
                          where s.Contains(a.Contract)
                          orderby a.ContractId
                          select a).ToList();
            }
        }
        static private void UpdateUkazanie(List<string> s)
        {
            using (MyDBContext db = new MyDBContext(ConfigClass.ConnectionString, ConfigClass.SqlProvider))
            {
                ukazaniya = (from a in db.Ukazaniya
                          where s.Contains(a.Ukazanie)
                          orderby a.UkazanieId
                          select a).ToList();
            }
        }
    }
}
