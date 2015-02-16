using System;
using NUnit.Framework;

namespace ClassLibrary1
{
    [TestFixture]
    public class ScssCompilerTests
    {
        private ScssCompiler _compiler;

        [SetUp]
        public void Setup()
        {
            _compiler = new ScssCompiler();
        }

        [Test]
        public void GiVenNotHing_ReturnsNotHing()
        {
            var output = _compiler.Compile(string.Empty);

            Assert.That(output, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Compile_SomeCss_ReturnsSameCss()
        {
            var css = "a { color: hotpink; }";

            var output = _compiler.Compile(css);

            Assert.AreEqual(css, output);
        }

        [TestCase("$myawesomecolour")]
        [TestCase("$myotherthing")]
        public void Compile_SassWithUndefinedVariable_AddsErrorComment(string variable)
        {
            var css = "a { color: " + variable + "; }";

            var output = _compiler.Compile(css);

            Assert.That(output, Is.StringContaining("/* Error: " + variable + " not defined. */"));
        }

        [Test]
        public void Compile_SassWithDefinedVariable_ReplacesVariable()
        {
            var css =
                "$myawesomecolour: hotpink;" + Environment.NewLine +
                "a { color: $myawesomecolour; }";

            var output = _compiler.Compile(css);
            
            Assert.That(output, Is.StringContaining("a { color: hotpink; }"));
        }
        
        [Test]
        public void Compile_SassWithDefinedVariable_RemovesVariableAssignment()
        {
            var css =
                "$myawesomecolour: hotpink;" + Environment.NewLine +
                "a { color: $myawesomecolour; }";

            var output = _compiler.Compile(css);

            Assert.That(output, Is.Not.StringContaining("$myawesomecolour: hotpink;"));
        }

        [Test]
        public void Compile_SassWithDefinedVariable_DoesntReplaceAssignmentDeclaration()
        {
            var css =
                "$myawesomecolour: hotpink;" + Environment.NewLine +
                "a { color: $myawesomecolour; }";

            var output = _compiler.Compile(css);

            Assert.That(output, Is.Not.StringContaining("hotpink: hotpink;"));
        }

        [Test]
        public void Compile_SassWithVariableThatIsPartOfCssExperssion_WorksOk()
        {
            var scss = "$font-stack: Comic Sans;" + Environment.NewLine + "font: 100% $font-stack;";

            var output = _compiler.Compile(scss);

            Assert.That(output, Is.StringContaining("font: 100% Comic Sans;"));
        }

        [TestCase("$var: value;")]
        [TestCase("$var   : value;")]
        [TestCase("$var:                value;")]
        [TestCase("$var:value;")]
        public void DebugCompile_VariableAssignmentsInBadShapes_StillRecognisesThem(string assignment)
        {
            var result = _compiler.DebugCompile(assignment);

            Assert.That(result.Variables.Count, Is.EqualTo(1));
        }
    }
}
