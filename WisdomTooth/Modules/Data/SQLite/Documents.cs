using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using MediaCurator.Common;
using System.Threading;

namespace MediaCurator.Data.SQLite
{
    public class Documents : MediaCurator.Data.SQLite.Table
    {

        internal Documents(Connection connection)
            : base(connection)
        {
        }

        protected override string GetCreateTableSql()
        {
            return "CREATE TABLE [Documents] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, [Type] BLOB NOT NULL, [Document] BLOB NOT NULL)";
        }

        public void Delete(int id)
        {
            Statement statement = Connection.GetStatement("DELETE FROM [Documents] WHERE [Id] = ?1;");
            try
            {
                statement.BindInt(1, id);

                using (var transaction = new Transaction(Connection))
                {
                    statement.Step();
                    transaction.Commit();
                }
            }
            finally
            {
                statement.ResetAndClearBindings();
            }
        }

        public void Write(IDocument document)
        {
            Debug.Assert(document != null, "5094031");

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                document.Serialize(new ProtobufEncoder(stream));
                bytes = stream.ToArray();
            }

            Statement statement;
            if (document.Id.HasValue)
            {
                statement = Connection.GetStatement("UPDATE [Documents] SET [Type] = ?2, [Document] = ?3 WHERE [Id] = ?1;");
            }
            else
            {
                statement = Connection.GetStatement("INSERT INTO [Documents] ([Type], [Document]) VALUES (?2, ?3);");
            }

            try
            {
                if (document.Id.HasValue)
                {
                    statement.BindInt(1, document.Id.Value);
                }

                statement.BindBytes(2, document.Type.ToByteArray());

                unsafe
                {
                    /* Buffer and its contents must remain valid until a new value is bound to the parameter, or the statement is finalize(/reset?). */
                    fixed (byte* pointer = bytes)
                    {
                        statement.BindBytes(3, new IntPtr(pointer), bytes.Length);

                        using (var transaction = new Transaction(Connection))
                        {
                            statement.Step();
                            transaction.Commit();
                        }
                    }
                }
            }
            finally
            {
                statement.ResetAndClearBindings();
            }

            var rows = SQLite.sqlite3_changes(Connection);
            Debug.Assert(rows == 1, "5224399");

            if (!document.Id.HasValue)
            {
                int newId = checked((int)SQLite.sqlite3_last_insert_rowid(Connection));
                document.SetIdOnce(newId);
            }
        }

        public T Read<T>(int id) where T : IDocument, new()
        {
            T result = default(T);
            byte[] bytes = null;

            Statement statement = Connection.GetStatement("SELECT [Id], [Type], [Document] FROM [Documents] WHERE [Id] = :1");
            try
            {
                statement.BindInt(1, id);

                using (var transaction = new Transaction(Connection))
                {
                    if (statement.TryStep())
                    {
                        result = new T();

                        result.SetIdOnce(statement.ReadInt(0));

                        var type = new Guid(statement.ReadBytes(1));
                        if (type != result.Type)
                        {
                            throw new MediaCuratorException("5135191");
                        }

                        bytes = statement.ReadBytes(2);
                    }

                    transaction.Commit();
                }
            }
            finally
            {
                statement.ResetAndClearBindings();
            }

            if (result != null)
            {
                using (var stream = new MemoryStream(bytes))
                {
                    result.Deserialize(new ProtobufDecoder(stream));
                }
            }
            else
            {
                throw new MediaCuratorException("5124594");
            }

            return result;
        }

    }
}
