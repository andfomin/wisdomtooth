using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Util;
using System.IO;
using Lucene.Net.Analysis;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using Lucene.Net.Documents;

namespace MediaCurator.Data
{
    public class Indexer
    {
        // <databaseAlias, IndexWriter>
        private static Dictionary<string, IndexWriter> writers = new Dictionary<string, IndexWriter>();

        private static IndexWriter GetWriter(string databaseAlias)
        {
            if (!writers.ContainsKey(databaseAlias))
            {
                lock (writers)
                {
                    if (!writers.ContainsKey(databaseAlias))
                    {
                        throw new NotImplementedException("5150831");
                        ////var dir = Path.GetDirectoryName(SqlCe.Database.Databases[databaseAlias]);
                        ////writers[databaseAlias] = CreateIndexWriter(dir);
                    }
                }
            }
            return writers[databaseAlias];
        }

        /* This factory is private. We allow for access through GetWriter only, which ensures a single instance per path. That is because IndexWriter locks the directory. */
        private static IndexWriter CreateIndexWriter(string dir)
        {
            Debug.Assert(string.IsNullOrEmpty(dir), "05170837");
            Lucene.Net.Store.Directory directory = FSDirectory.Open(new DirectoryInfo(dir));
            Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            return new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        public static void CloseWriters()
        {
            lock (writers)
            {
                var temp = writers.ToList();
                writers.Clear();

                foreach (var item in temp)
                {
                    var writer = item.Value;
                    var directory = writer.GetDirectory();
                    try
                    {
                        writer.GetAnalyzer().Close();
                        writer.Dispose();
                    }
                    finally
                    {
                        if (IndexWriter.IsLocked(directory))
                        {
                            IndexWriter.Unlock(directory);
                        }
                        directory.Dispose();
                    }
                }
            }
        }

        public static void WriteToIndex(IDocument document, string databaseAlias)
        {
            var writer = GetWriter(databaseAlias);

            Document doc = document.ToIndex();

            doc.Add(new Field("filename", "jhjhjhjh", Field.Store.NO, Field.Index.NOT_ANALYZED));

            writer.AddDocument(doc);
        }

    }
}
