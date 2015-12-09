using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using MediaCurator.Common;
using System.IO;

namespace MediaCurator.Controller
{

    public sealed class DatabaseInitiator : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            string commonDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            //string dataDirectory = Path.Combine(commonDir, Constants.MediaCuratorCommonDirectoryName);

        
        }
    }
}
