# Contributing to NexusStack

Thank you for your interest in contributing to NexusStack! 🎉

## How to Contribute

### Reporting Issues

- Use the GitHub issue tracker
- Check if the issue already exists
- Provide clear reproduction steps
- Include your environment details (.NET version, OS, etc.)

### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# coding conventions
- Write clear, self-documenting code
- Add XML documentation for public APIs
- Include unit tests for new features
- Ensure all tests pass before submitting

### Template Guidelines

- Maintain backward compatibility when possible
- Document all template parameters
- Test on multiple platforms (Windows, Linux, macOS)
- Update documentation for any changes

## Development Setup

```powershell
# Clone the repository
git clone https://github.com/yongpengW/NexusStack.git

# Install the template locally
cd NexusStack
.\Install-Template.ps1

# Test the template
dotnet new nexusstack -n TestProject
```

## Questions?

Feel free to open an issue for any questions or discussions.

Thank you for contributing! 🚀
