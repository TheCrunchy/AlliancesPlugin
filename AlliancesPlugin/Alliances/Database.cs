﻿using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AlliancesPlugin.Alliances
{
    public class DatabaseForBank
    {

        public static bool ReadyToSave = true;

        //public static void testConnection()
        //{ 
        //    MySqlConnection conn = new MySqlConnection(connStr);
        //    try
        //    {
        //        AlliancePlugin.Log.Info("Connecting to MySQL...");
        //        conn.Open();


        //        string sql = "CREATE DATABASE IF NOT EXISTS AllianceBank";
        //        MySqlCommand cmd = new MySqlCommand(sql, conn);
        //        cmd.ExecuteNonQuery();
        //        sql = "CREATE TABLE IF NOT EXISTS AllianceBank.BankBalances(allianceId CHAR(36) PRIMARY KEY, balance BIGINT)";
        //        cmd = new MySqlCommand(sql, conn);
        //        cmd.ExecuteNonQuery();
        //        AlliancePlugin.Log.Info("Created the tables if it needed to");
        //    }
        //    catch (Exception ex)
        //    {
        //        ReadyToSave = false;
        //        conn.Close();
        //        AlliancePlugin.Log.Error("Error connecting to database, disabling bank features.");
        //        AlliancePlugin.Log.Error(ex);
        //    }
        //    conn.Close();
        //    AlliancePlugin.Log.Info("Successfully connected to database!");
        //}
        public static String connectionString = "Filename=" + AlliancePlugin.path + "//bank.db;Connection=Direct;Upgrade=True;";
        public static Boolean CreateAllianceBank(Alliance alliance)
        {
         //   if (!File.Exists(AlliancePlugin.path + "//bank.db"))
          //  {
          //      File.Create(AlliancePlugin.path + "//bank.db");
          //  }
            try
            {
                using (var db = new LiteDatabase(connectionString))
                {
                    var collection = db.GetCollection<BankData>("BankData");
                    var bank = new BankData
                    {
                        Id = alliance.AllianceId,
                        balance = 1
                    };
                    collection.Insert(bank);

                }

            }
            catch (Exception ex)
            {
                AlliancePlugin.Log.Error("Error creating new alliance in db");
                AlliancePlugin.Log.Error(ex);
                return false;
            }
            return true;
            //MySqlConnection conn = new MySqlConnection(connStr);
            //try
            //{
            //    AlliancePlugin.Log.Info("Connecting to MySQL...");
            //    conn.Open();


            //    string sql = "INSERT INTO AllianceBank.BankBalances(allianceId, balance) VALUES ROW ('" + alliance.AllianceId.ToString() +  "',0)";
            //    MySqlCommand cmd = new MySqlCommand(sql, conn);
            //    cmd.ExecuteNonQuery();
            //    AlliancePlugin.Log.Info("Inserted a new alliance into the bank");
            //}
            //catch (Exception ex)
            //{
            //    conn.Close();
            //    AlliancePlugin.Log.Error("Error adding new alliance to bank.");
            //    AlliancePlugin.Log.Error(ex);
            //}
            //conn.Close();
            //AlliancePlugin.Log.Info("Successfully created bank data in database!");
            //return true;
        }
        public static Boolean PayShipyardFee(Alliance alliance, long fee, ulong id)
        {
            try
            {
                using (var db = new LiteDatabase(connectionString))
                {
                    var collection = db.GetCollection<BankData>("BankData");
                    var bank = collection.FindById(alliance.AllianceId);
                    if (bank == null)
                    {
                        bank = new BankData
                        {
                            Id = alliance.AllianceId,
                            balance = fee
                        };
                        collection.Insert(bank);

                    }
                    else
                    {
                        bank.balance += fee;

                        collection.Update(bank);
                    }
                 
                    
                }

            }
            catch (Exception ex)
            {
                AlliancePlugin.Log.Error("Error with adding shipyard fee to bank");
                AlliancePlugin.Log.Error(ex);
                return false;
            }
            return true;
            //MySqlConnection conn = new MySqlConnection(connStr);
            //try
            //{
            //    //Update the players table to add a new player and contract, if the player exists in table, update the contract id field
            //    //currently a player can only ever have one active contract, but that would be fairly easy to change
            //    //throw the player id into the contracts table then load all of those where completed is false
            //    conn.Open();
            //   String sql = "UPDATE AllianceBank.BankBalances SET balance = balance + " + fee + " where allianceId =" + alliance.AllianceId.ToString();
            //    MySqlCommand cmd = new MySqlCommand(sql, conn);
            //    cmd.ExecuteNonQuery();

            //}
            //catch (Exception ex)
            //{
            //    conn.Close();
            //   AlliancePlugin.Log.Error("Error on paying shipyard fee", ex.ToString());
            //    return false;
            //}
            //conn.Close();
            //AlliancePlugin.Log.Info("Paid shipyard fee " + id);
            //return true;
        }

        public static Boolean AddToBalance(Alliance alliance, long amount)
        {
            try
            {
                using (var db = new LiteDatabase(connectionString))
                {
                    var collection = db.GetCollection<BankData>("BankData");
                    var bank = collection.FindById(alliance.AllianceId);
                    if (bank == null)
                    {
                        bank = new BankData
                        {
                            Id = alliance.AllianceId,
                            balance = amount
                        };
                        collection.Insert(bank);

                    }
                    else
                    {
                        bank.balance += amount;

                        collection.Update(bank);
                    }
                 

                }

            }
            catch (Exception ex)
            {
                AlliancePlugin.Log.Error("Error with adding money to bank");
                AlliancePlugin.Log.Error(ex);
                return false;
            }
            return true;
        }
        public static Boolean RemoveFromBalance(Alliance alliance, long amount)
        {
            try
            {
                using (var db = new LiteDatabase(connectionString))
                {
                    var collection = db.GetCollection<BankData>("BankData");
                    var bank = collection.FindById(alliance.AllianceId);
                    if (bank == null)
                    {
                        bank = new BankData
                        {
                            Id = alliance.AllianceId,
                            balance = 0
                        };
                        collection.Insert(bank);

                    }
                    else
                    {
                        bank.balance -= amount;

                        collection.Update(bank);
                    }
                }

            }
            catch (Exception ex)
            {
                AlliancePlugin.Log.Error("Error with removing money from bank");
                AlliancePlugin.Log.Error(ex);
                return false;
            }
            return true;
        }

        public static long GetBalance(Guid allianceId)
        {
            try
            {
                using (var db = new LiteDatabase(connectionString))
                {
                    var collection = db.GetCollection<BankData>("BankData");
                    var bank = collection.FindById(allianceId);
                    if (bank == null)
                    {

                        bank = new BankData
                        {
                            Id = allianceId,
                            balance = 0
                        };
                        collection.Insert(bank);


                        return 0;
                    }
                    else
                    {
                        return bank.balance;
                    }
                }

            }
            catch (Exception ex)
            {
                AlliancePlugin.Log.Error("Error with adding shipyard fee to bank");
                AlliancePlugin.Log.Error(ex);
                return 0;
            }
            return 0;
        }
    }
}