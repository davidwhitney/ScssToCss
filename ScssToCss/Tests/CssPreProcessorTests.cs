using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace ClassLibrary1.Tests
{
    public class CssPreProcessorTests
    {
        private Mock<IFileSystem> _fs;
        private CssPreProcessor _preprocessor;
        private Mock<FileBase> _file;

        [SetUp]
        public void SetUp()
        {
            _fs = new Mock<IFileSystem>();
            _file = new Mock<FileBase>();
            _fs.Setup(x => x.File).Returns(_file.Object);

            _file.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            _file.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns("");

            _preprocessor = new CssPreProcessor(_fs.Object);
        }

        [Test]
        public void BundleAndCompile_GivenEmptyScssFile_OutputsCssFile()
        {
            var result = _preprocessor.BundleAndCompile("abc.scss");

            Assert.That(result.First().Key, Is.EqualTo("abc.css"));
            Assert.That(result.First().Value, Is.EqualTo(""));
        }

        [Test]
        public void BundleAndCompile_GivenMissingFile_ThrowsFileNotFoundException()
        {
            _file.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            
            var ex = Assert.Throws<FileNotFoundException>(
                () => _preprocessor.BundleAndCompile("abc.scss"));

            Assert.That(ex.Message, Is.StringContaining("'abc.scss' not found."));
        }

        [Test]
        public void BundleAndCompile_GivenScssFileWhichOnlyHasValidCssInIt_OutputsCssFile()
        {
            var css = "a { color: hotpink; }";
            _file.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(css);

            var result = _preprocessor.BundleAndCompile("abc.scss");

            Assert.That(result.First().Key, Is.EqualTo("abc.css"));
            Assert.That(result.First().Value, Is.EqualTo(css));
        }
        
        [Test]
        public void BundleAndCompile_GivenScssFileWithScssContent_OutputsProcessedContents()
        {
            var css =
                "$myawesomecolour: hotpink;" + Environment.NewLine +
                "a { color: $myawesomecolour; }";

            _file.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(css);

            var result = _preprocessor.BundleAndCompile("abc.scss");

            Assert.That(result.First().Key, Is.EqualTo("abc.css"));
            Assert.That(result.First().Value, Is.EqualTo("a { color: hotpink; }"));
        }

        [Test]
        public void BundleAndCompile_GivenFilePrefixedWithUnderscore_DoesNotOutputCssFile()
        {
            var result = _preprocessor.BundleAndCompile("_my-rocking-partial.scss");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void BundleAndCompile_GivenImportWithMissingFile_ThrowsFileNotFoundException()
        {
            _file.Setup(x => x.ReadAllText("some.scss"))
                .Returns("@import 'non-existant';");
            _file.Setup(x => x.ReadAllText("non-existant.scss"))
                .Throws<FileNotFoundException>();

            var ex = Assert.Throws<FileNotFoundException>(
                () => _preprocessor.BundleAndCompile("some.scss"));

            Assert.That(ex.Message, Is.StringContaining("File to import not found: 'non-existant'."));
        }
    }
}