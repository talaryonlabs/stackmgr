using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Validation;

public class ValidationException(string message) : UserErrorException(message);

public class StackNameValidationException(string message) : ValidationException(message);

public class HostnameValidationException(string message) : ValidationException(message);

public class SizeValidationException(string message) : ValidationException(message);

public class PortValidationException(string message) : ValidationException(message);

public class UrlValidationException(string message) : ValidationException(message);

public class ImageNameValidationException(string message) : ValidationException(message);

public class NamespaceValidationException(string message) : ValidationException(message);

public class AppNameValidationException(string message) : ValidationException(message);
