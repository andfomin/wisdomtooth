using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MediaCurator.Data.SQLite
{
    public abstract class Table
    {
        // The execution plan is cached by the command object. The engine does not cache execution plans.
        //private Dictionary<string, SqlCeCommand> commandCache = new Dictionary<string, SqlCeCommand>();

        public virtual string TableName
        {
            get
            {
                return GetType().Name;
            }
        }

        protected Connection Connection { get; private set; }

        public Table(Connection connection)
        {
            this.Connection = connection;
        }


        /// <summary>
        /// Checks correspondence between the code schema and the database schema.
        /// </summary>
        /// <param name="enforce">If the initial schema is wrong, makes an attempt to recreate the table.</param>
        /// <returns>True if the schema is OK</returns>
        internal bool IsSchemaOk(bool enforce)
        {
            var result = CheckSchema();
            if (!result && enforce)
            {
                DeleteTable();
                CreateTable();
                result = this.CheckSchema();
                Trace.TraceInformation("Table {0} recreated. Schema: {1}", this.TableName, result);
            }
            return result;
        }

        private void DeleteTable()
        {
            this.Connection.ExecuteStatementOnce(string.Format("DROP TABLE IF EXISTS {0}", TableName), "5165763");
        }

        private void CreateTable()
        {
            this.Connection.ExecuteStatementOnce(GetCreateTableSql(), "5162150");
        }

        private bool CheckSchema()
        {
            bool result;
            string codedSql = GetCreateTableSql();
            string sql = string.Format("SELECT [sql] FROM [sqlite_master] WHERE type='table' AND name = '{0}'", TableName);
            using (var statement = Connection.GetStatement(sql))
            {
                result = statement.TryStep();
                if (result)
                {
                    string realSql = statement.ReadText(0);
                    result = realSql == codedSql;
                }
            }
            return result;
            /* For indices, type is equal to 'index', name is the name of the index and tbl_name is the name of the table to which the index belongs. For both tables and indices, the sql field is the text of the original CREATE TABLE or CREATE INDEX statement that created the table or index. For automatically created indices (used to implement the PRIMARY KEY or UNIQUE constraints) the sql field is NULL. */
        }

        protected abstract string GetCreateTableSql();

    }
}
