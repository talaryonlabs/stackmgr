namespace stackmgr.Exceptions;

public class TemplateNotFoundException(string? name = null) : Exception (name is not null ? $"Template '{name}' not found." : "Template not found.");