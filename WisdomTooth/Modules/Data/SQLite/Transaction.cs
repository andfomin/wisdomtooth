using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MediaCurator.Data.SQLite
{
    public class Transaction : IDisposable
    {
        private Connection connection;

        public Transaction(Connection connection)
        {
            this.connection = connection;
            connection.AddTransaction(this);
            Execute(this.connection, "BEGIN EXCLUSIVE");
        }

        private void Execute(Connection connection, string sql)
        {
            connection.GetStatement(sql).ExecuteConcurrently();
        }

        private void Finish(string sql)
        {
            if (this.connection != null)
            {
                var conn = this.connection;
                this.connection = null;
                conn.RemoveTransaction(this);
                Execute(conn, sql);
            }
        }

        public void Commit()
        {
            Finish("COMMIT");
        }

        public void Rollback()
        {
            Finish("ROLLBACK");
        }

        public void Dispose()
        {
            Rollback();
        }
    }
}
