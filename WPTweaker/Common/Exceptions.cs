using System;

namespace WPTweaker
{
    public class AttributeMissingException : Exception
    {
        public AttributeMissingException() : base("Error, no attributes found") {}
        public AttributeMissingException(string attrName) : base(string.Format("Error: attribute \"{0}\" is missing", attrName)) { }
        public AttributeMissingException(string attrName, string elementName) : base(string.Format("Error: attribute \"{0}\" is missing for the element \"{1}\"", attrName, elementName)) { }
    }

    public class InvalidArgumentTypeException : InvalidOperationException
    {
        public InvalidArgumentTypeException() : base() { }
        public InvalidArgumentTypeException(string message) : base(message) { }
    }
}
