using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Validation;

public class ValidationException : UserErrorException
{
    public ValidationException(string message) : base(message) { }
}

public class StackNameValidationException : ValidationException
{
    public StackNameValidationException(string message) : base(message) { }
}

public class HostnameValidationException : ValidationException
{
    public HostnameValidationException(string message) : base(message) { }
}

public class SizeValidationException : ValidationException
{
    public SizeValidationException(string message) : base(message) { }
}

public class PortValidationException : ValidationException
{
    public PortValidationException(string message) : base(message) { }
}

public class UrlValidationException : ValidationException
{
    public UrlValidationException(string message) : base(message) { }
}

public class ImageNameValidationException : ValidationException
{
    public ImageNameValidationException(string message) : base(message) { }
}

public class NamespaceValidationException : ValidationException
{
    public NamespaceValidationException(string message) : base(message) { }
}

public class AppNameValidationException : ValidationException
{
    public AppNameValidationException(string message) : base(message) { }
}
