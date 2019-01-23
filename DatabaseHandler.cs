using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlServerCe;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;
using Inventory_monitor.Models;

namespace Inventory_monitor.Controls
{
    class DatabaseHandler
    {
        const string DATABASENAME = "INVENTORYMONITORDATABASE";
        static string databasePath = MainWindow.dataFolder + "Database/";
        public static string databaseFile = databasePath + DATABASENAME + ".sdf";
        static string connString = "Data Source='" + databaseFile + "'";
        SqlCeCommand cmd;
        SqlCeConnection conn = new SqlCeConnection(connString);
        MainWindow mainWindow;

        private const string RES_TABLE_NAME = "resTable";
        private const string TRANS_TABLE_NAME = "transTable";
        private const string COLUMN_ID = "id";
        private const string COLUMN_PAR_ID = "parentId";
        private const string COLUMN_TITLE = "title";
        private const string COLUMN_DESCRIPTION = "description";
        private const string COLUMN_IS_GROUP = "isGroup";
        private const string COLUMN_INIT_SIZE = "initialSize";
        private const string COLUMN_IMAGE_PATH = "imagePath";
        private const string COLUMN_DATE_CREATED = "dateCreated";
        private const string COLUMN_IS_PINNED = "isPinned";
        private const string COLUMN_PINNED_DATE = "pinnedDate";

        private const string COLUMN_IS_REMOVAL = "isRemoval";
        private const string COLUMN_COMMENT = "comment";
        private const string COLUMN_TRANS_NUM = "numberTransacted";
        private const string COLUMN_SHORT_DATE = "shortDate";

        public DatabaseHandler(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
        public static bool CreateNewDatabase()
        {
            try
            {
                if (!Directory.Exists(databasePath))
                    Directory.CreateDirectory(databasePath);
                if (File.Exists(databaseFile))
                    File.Delete(databaseFile);
                SqlCeEngine engine = new SqlCeEngine(connString);
                engine.CreateDatabase();
                return CreateTables();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
        static bool CreateTables()
        {
            SqlCeConnection conn = new SqlCeConnection(connString);
            SqlCeCommand cmd;
            try
            {
                conn.Open();
                cmd = conn.CreateCommand();
                string update = $"create table {RES_TABLE_NAME}("
                        + $"{COLUMN_ID} int not null identity(1,1) primary key,"
                        + $"{COLUMN_PAR_ID} int not null,"
                        + $"{COLUMN_TITLE} nvarchar(200) not null,"
                        + $"{COLUMN_DESCRIPTION} nvarchar(4000),"
                        + $"{COLUMN_IS_GROUP} bit not null,"
                        + $"{COLUMN_INIT_SIZE} int,"
                        + $"{COLUMN_IMAGE_PATH} nvarchar(1000),"
                        + $"{COLUMN_DATE_CREATED} nvarchar(200),"
                        + $"{COLUMN_IS_PINNED} bit not null,"
                        + $"{COLUMN_PINNED_DATE} bigint)";
                cmd.CommandText = update;
                cmd.ExecuteNonQuery();
                update = $"create table {TRANS_TABLE_NAME}("
                        + $"{COLUMN_ID} int not null identity(1,1) primary key,"
                        + $"{COLUMN_PAR_ID} int not null,"
                        + $"{COLUMN_COMMENT} nvarchar(4000),"
                        + $"{COLUMN_IS_REMOVAL} bit not null,"
                        + $"{COLUMN_TRANS_NUM} int,"
                        + $"{COLUMN_DATE_CREATED} nvarchar(200),"
                        + $"{COLUMN_SHORT_DATE} nvarchar(100))";
                cmd.CommandText = update;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
                conn.Close();
                return false;
            }
            return true;
        }

        public bool ExecuteNonQuery(string update)
        {
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = update;
                cmd.ExecuteNonQuery();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
                conn.Close();
                return false;
            }
            return true;
        }

        public SqlCeDataReader ExecuteQuery(string query)
        {
            SqlCeDataReader reader = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                reader = cmd.ExecuteReader();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
                conn.Close();
            }
            return reader;
        }
        public void Open()
        {
            conn.Open();
        }
        public void Close()
        {
            conn.Close();
        }

        public ObservableCollection<Resource> GetResources()
        {
            ObservableCollection<Resource> resources = new ObservableCollection<Resource>();
            string query = $"select * from {RES_TABLE_NAME} where {COLUMN_PAR_ID} = 0 order by {COLUMN_PINNED_DATE} desc";
            Open();
            SqlCeDataReader reader = ExecuteQuery(query);
            while (reader.Read())
            {
                Resource res = ReaderToResource(reader);
                PopulateResource(res);
                resources.Add(res);
            }
            return resources;
        }

        void PopulateResource(Resource res)
        {
            if (res.IsGroup)
            {
                string query = $"select * from {RES_TABLE_NAME} where {COLUMN_PAR_ID} = {res.Id} order by {COLUMN_PINNED_DATE} desc";
                SqlCeDataReader reader = ExecuteQuery(query);
                while (reader.Read())
                {
                    Resource resource = ReaderToResource(reader);
                    PopulateResource(resource);
                    res.AddResource(resource);
                }
            }
            else
            {
                string query = $"select * from {TRANS_TABLE_NAME} where {COLUMN_PAR_ID} = {res.Id}";
                SqlCeDataReader reader = ExecuteQuery(query);
                while (reader.Read())
                {
                    Transaction transaction = ReaderToTransaction(reader, res);
                    res.AddTransaction(transaction);
                }
            }
        }

        Resource ReaderToResource(SqlCeDataReader reader)
        {
            string image = reader.IsDBNull(reader.GetOrdinal(COLUMN_IMAGE_PATH)) ? null :
                reader.GetString(reader.GetOrdinal(COLUMN_IMAGE_PATH));

            return new Resource(reader.GetInt32(reader.GetOrdinal(COLUMN_ID)), reader.GetInt32(reader.GetOrdinal(COLUMN_PAR_ID)),
                reader.GetString(reader.GetOrdinal(COLUMN_TITLE)), reader.GetString(reader.GetOrdinal(COLUMN_DESCRIPTION)),
                reader.GetInt32(reader.GetOrdinal(COLUMN_INIT_SIZE)), reader.GetBoolean(reader.GetOrdinal(COLUMN_IS_PINNED)),
                reader.GetInt64(reader.GetOrdinal(COLUMN_PINNED_DATE)), image,
                reader.GetBoolean(reader.GetOrdinal(COLUMN_IS_GROUP)), reader.GetString(reader.GetOrdinal(COLUMN_DATE_CREATED)),
                mainWindow);
        }

        Transaction ReaderToTransaction(SqlCeDataReader reader, Resource par)
        {
            return new Transaction(reader.GetInt32(reader.GetOrdinal(COLUMN_ID)), reader.GetInt32(reader.GetOrdinal(COLUMN_PAR_ID)),
                reader.GetBoolean(reader.GetOrdinal(COLUMN_IS_REMOVAL)), reader.GetInt32(reader.GetOrdinal(COLUMN_TRANS_NUM)), par,
                reader.GetString(reader.GetOrdinal(COLUMN_COMMENT)), reader.GetString(reader.GetOrdinal(COLUMN_DATE_CREATED)),
                reader.GetString(reader.GetOrdinal(COLUMN_SHORT_DATE)));
        }
        
        public Resource AddResource(string title, string desc, int stock, string imagePath, Resource parent)
        {
            title = title.Length > 200 ? title.Substring(0, 200) : title;
            desc = desc.Length > 4000 ? desc.Substring(0, 3999) : desc;

            string insert = $"insert into {RES_TABLE_NAME} ({COLUMN_PAR_ID}, {COLUMN_TITLE}, {COLUMN_DESCRIPTION}, {COLUMN_IS_GROUP}," +
                $"{COLUMN_INIT_SIZE}, {COLUMN_IMAGE_PATH}, {COLUMN_DATE_CREATED}, {COLUMN_IS_PINNED}, {COLUMN_PINNED_DATE}) " +
                "values(@COLUMN_PAR_ID, @COLUMN_TITLE, @COLUMN_DESCRIPTION, @COLUMN_IS_GROUP, @COLUMN_INIT_SIZE, @COLUMN_IMAGE_PATH," +
                " @COLUMN_DATE_CREATED, @COLUMN_IS_PINNED, @COLUMN_PINNED_DATE)";

            string query = $"select * from {RES_TABLE_NAME} where {COLUMN_ID} = @@IDENTITY";

            DateTime curr = DateTime.Now;

            string fullDate = curr.ToShortTimeString() + ", " + curr.ToLongDateString().Split(',')[1].Trim();
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = insert;
                cmd.Parameters.AddWithValue("@COLUMN_PAR_ID", parent == null ? 0 : parent.Id);
                cmd.Parameters.AddWithValue("@COLUMN_TITLE", title);
                cmd.Parameters.AddWithValue("@COLUMN_DESCRIPTION", desc);
                cmd.Parameters.AddWithValue("@COLUMN_IS_GROUP", false);
                cmd.Parameters.AddWithValue("@COLUMN_INIT_SIZE", stock);
                cmd.Parameters.AddWithValue("@COLUMN_IMAGE_PATH", imagePath == null ? Convert.DBNull : imagePath);
                cmd.Parameters.AddWithValue("@COLUMN_DATE_CREATED", fullDate);
                cmd.Parameters.AddWithValue("@COLUMN_IS_PINNED", false);
                cmd.Parameters.AddWithValue("@COLUMN_PINNED_DATE", 0);
                cmd.ExecuteNonQuery();

                SqlCeDataReader reader = ExecuteQuery(query);
                reader.Read();
                return ReaderToResource(reader);
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public Resource AddGroup(string title, string desc, Resource parent, ObservableCollection<Resource> resources)
        {
            title = title.Length > 200 ? title.Substring(0, 200) : title;
            desc = desc.Length > 4000 ? desc.Substring(0, 3999) : desc;

            string insert = $"insert into {RES_TABLE_NAME} ({COLUMN_PAR_ID}, {COLUMN_TITLE}, {COLUMN_DESCRIPTION}, {COLUMN_IS_GROUP}," +
                $"{COLUMN_INIT_SIZE}, {COLUMN_IMAGE_PATH}, {COLUMN_DATE_CREATED}, {COLUMN_IS_PINNED}, {COLUMN_PINNED_DATE}) " +
                "values(@COLUMN_PAR_ID, @COLUMN_TITLE, @COLUMN_DESCRIPTION, @COLUMN_IS_GROUP, @COLUMN_INIT_SIZE, @COLUMN_IMAGE_PATH," +
                " @COLUMN_DATE_CREATED, @COLUMN_IS_PINNED, @COLUMN_PINNED_DATE)";

            string query = $"select * from {RES_TABLE_NAME} where {COLUMN_ID} = @@IDENTITY";

            DateTime curr = DateTime.Now;

            string fullDate = curr.ToShortTimeString() + ", " + curr.ToLongDateString().Split(',')[1].Trim();
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = insert;
                cmd.Parameters.AddWithValue("@COLUMN_PAR_ID", parent == null ? 0 : parent.Id);
                cmd.Parameters.AddWithValue("@COLUMN_TITLE", title);
                cmd.Parameters.AddWithValue("@COLUMN_DESCRIPTION", desc);
                cmd.Parameters.AddWithValue("@COLUMN_IS_GROUP", true);
                cmd.Parameters.AddWithValue("@COLUMN_INIT_SIZE", 0);
                cmd.Parameters.AddWithValue("@COLUMN_IMAGE_PATH", Convert.DBNull);
                cmd.Parameters.AddWithValue("@COLUMN_DATE_CREATED", fullDate);
                cmd.Parameters.AddWithValue("@COLUMN_IS_PINNED", false);
                cmd.Parameters.AddWithValue("@COLUMN_PINNED_DATE", 0);
                cmd.ExecuteNonQuery();

                SqlCeDataReader reader = ExecuteQuery(query);
                reader.Read();
                Resource group = ReaderToResource(reader);
                foreach (Resource resource in resources)
                {
                    UpdateResParent(resource, group);
                }
                return group;
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public Transaction AddTransaction(bool isRemoval, int transNum, string comment, Resource parent)
        {
            comment = comment.Length > 4000 ? comment.Substring(0, 3999) : comment;

            string insert = $"insert into {TRANS_TABLE_NAME} ({COLUMN_PAR_ID}, {COLUMN_COMMENT},{COLUMN_IS_REMOVAL},{COLUMN_TRANS_NUM}," +
                $" {COLUMN_DATE_CREATED}, {COLUMN_SHORT_DATE}) values(@COLUMN_PAR_ID, @COLUMN_COMMENT, @COLUMN_IS_REMOVAL, " +
                $"@COLUMN_TRANS_NUM, @COLUMN_DATE_CREATED," +
                $" @COLUMN_SHORT_DATE)";

            string query = $"select * from {TRANS_TABLE_NAME} where {COLUMN_ID} = @@IDENTITY";

            DateTime curr = DateTime.Now;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = insert;
                cmd.Parameters.AddWithValue("@COLUMN_PAR_ID", parent.Id);
                cmd.Parameters.AddWithValue("@COLUMN_COMMENT", comment);
                cmd.Parameters.AddWithValue("@COLUMN_IS_REMOVAL", isRemoval);
                cmd.Parameters.AddWithValue("@COLUMN_TRANS_NUM", transNum);
                cmd.Parameters.AddWithValue("@COLUMN_DATE_CREATED", curr.ToShortTimeString() + ", " + curr.ToLongDateString().Split(',')[1].Trim());
                cmd.Parameters.AddWithValue("@COLUMN_SHORT_DATE", curr.ToShortDateString());
                cmd.ExecuteNonQuery();

                SqlCeDataReader reader = ExecuteQuery(query);
                reader.Read();
                return ReaderToTransaction(reader, parent);
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public void DeleteResource(Resource resource)
        {
            string delete = $"delete from {RES_TABLE_NAME} where id = {resource.Id}";
            ExecuteNonQuery(delete);
        }
        public void DeleteTransaction(Transaction transaction)
        {
            string delete = $"delete from {TRANS_TABLE_NAME} where id = {transaction.Id}";
            ExecuteNonQuery(delete);
        }
        public void UpdateResParent(Resource res, Resource parent)
        {
            string update = $"update {RES_TABLE_NAME} set {COLUMN_PAR_ID} = {(parent == null ? 0 : parent.Id)} where {COLUMN_ID} = {res.Id}";
            try
            {
                ExecuteNonQuery(update);
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void SetPinned(Resource resource, bool isPinned)
        {
            long pD = isPinned ? (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) : 0;
            string update = $"update {RES_TABLE_NAME} set {COLUMN_IS_PINNED} = @COLUMN_IS_PINNED, {COLUMN_PINNED_DATE} = @COLUMN_PINNED_DATE where {COLUMN_ID} = {resource.Id}";
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = update;
                cmd.Parameters.AddWithValue("@COLUMN_IS_PINNED", isPinned);
                cmd.Parameters.AddWithValue("@COLUMN_PINNED_DATE", pD);
                cmd.ExecuteNonQuery();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void UpdateTitle(Resource resource, string title)
        {
            title = title.Length > 200 ? title.Substring(0, 200) : title;
            string update = $"update {RES_TABLE_NAME} set {COLUMN_TITLE} = @COLUMN_TITLE where {COLUMN_ID} = {resource.Id}";
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = update;
                cmd.Parameters.AddWithValue("@COLUMN_TITLE", title);
                cmd.ExecuteNonQuery();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void UpdateDescription(Resource resource, string description)
        {
            description = description.Length > 4000 ? description.Substring(0, 3999) : description;
            string update = $"update {RES_TABLE_NAME} set {COLUMN_DESCRIPTION} = @COLUMN_DESCRIPTION where {COLUMN_ID} = {resource.Id}";
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = update;
                cmd.Parameters.AddWithValue("@COLUMN_DESCRIPTION", description);
                cmd.ExecuteNonQuery();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void UpdatePicture(Resource resource, string imagePath)
        {
            string update = $"update {RES_TABLE_NAME} set {COLUMN_IMAGE_PATH} = @COLUMN_IMAGE_PATH where {COLUMN_ID} = {resource.Id}";
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = update;
                cmd.Parameters.AddWithValue("@COLUMN_IMAGE_PATH", imagePath == null ? Convert.DBNull : imagePath);
                cmd.ExecuteNonQuery();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void UpdateComment(Transaction transaction, string comment)
        {
            comment = comment.Length > 4000 ? comment.Substring(0, 3999) : comment;
            string update = $"update {TRANS_TABLE_NAME} set {COLUMN_COMMENT} = @COLUMN_COMMENT where {COLUMN_ID} = {transaction.Id}";
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = update;
                cmd.Parameters.AddWithValue("@COLUMN_COMMENT", comment);
                cmd.ExecuteNonQuery();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
