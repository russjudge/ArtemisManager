using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction.Tests
{
    [TestClass()]
    public class ArtemisManagerTests
    {
        [TestMethod()]
        public void ResolveFilenameTest()
        {
            string tar = ArtemisManager.ResolveFilename(ArtemisManager.ArtemisINIFolder, "Original(1)", ArtemisManager.INIFileExtension);

        }
    }
}