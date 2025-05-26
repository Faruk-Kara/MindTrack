# Contributing to MindTrack 🤝

Thank you for your interest in contributing to MindTrack! We welcome contributions from everyone, whether you're fixing bugs, adding features, improving documentation, or suggesting enhancements.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Issue Reporting](#issue-reporting)
- [Feature Requests](#feature-requests)

## 📜 Code of Conduct

This project adheres to a code of conduct that we expect all contributors to follow:

- **Be respectful**: Treat everyone with respect and kindness
- **Be inclusive**: Welcome newcomers and help them get started
- **Be constructive**: Provide helpful feedback and suggestions
- **Be patient**: Remember that everyone has different skill levels and backgrounds

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have:

- **Windows 10/11** (required for Windows Forms development)
- **Visual Studio 2019/2022** with .NET Framework development workload
- **.NET Framework 4.7.2** or later
- **Git** for version control
- Basic knowledge of **C#** and **Windows Forms**

### Development Setup

1. **Fork the repository**
   ```bash
   # Click the "Fork" button on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/MindTrack.git
   cd MindTrack
   ```

2. **Set up the upstream remote**
   ```bash
   git remote add upstream https://github.com/ORIGINAL_OWNER/MindTrack.git
   ```

3. **Open the project**
   - Open `MindTrack.sln` in Visual Studio
   - Restore NuGet packages (should happen automatically)
   - Build the solution to ensure everything works

4. **Create a branch for your work**
   ```bash
   git checkout -b feature/your-feature-name
   ```

## 🛠️ How to Contribute

### Types of Contributions

We welcome various types of contributions:

- **🐛 Bug fixes**: Fix issues and improve stability
- **✨ New features**: Add new functionality to enhance the application
- **📚 Documentation**: Improve README, code comments, or create tutorials
- **🎨 UI/UX improvements**: Enhance the user interface and experience
- **⚡ Performance optimizations**: Make the application faster and more efficient
- **🧪 Tests**: Add unit tests or integration tests
- **🔧 Refactoring**: Improve code structure and maintainability

### Before You Start

1. **Check existing issues**: Look for existing issues or feature requests
2. **Create an issue**: If your contribution is significant, create an issue first to discuss it
3. **Get feedback**: Wait for maintainer feedback before starting major work

## 🔄 Pull Request Process

### 1. Prepare Your Changes

- Ensure your code follows the [coding standards](#coding-standards)
- Add or update tests if applicable
- Update documentation if needed
- Test your changes thoroughly

### 2. Commit Your Changes

Use clear, descriptive commit messages:

```bash
# Good commit messages
git commit -m "Add mood trend visualization to statistics form"
git commit -m "Fix database connection timeout issue"
git commit -m "Update README with new installation instructions"

# Poor commit messages (avoid these)
git commit -m "fix bug"
git commit -m "update stuff"
git commit -m "changes"
```

### 3. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub with:

- **Clear title**: Describe what your PR does
- **Detailed description**: Explain the changes and why they're needed
- **Screenshots**: Include screenshots for UI changes
- **Testing notes**: Describe how you tested your changes

### 4. Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Performance improvement
- [ ] Code refactoring

## Testing
- [ ] I have tested these changes locally
- [ ] I have added/updated tests as needed
- [ ] All existing tests pass

## Screenshots (if applicable)
Add screenshots here

## Additional Notes
Any additional information or context
```

## 📝 Coding Standards

### C# Coding Conventions

Follow Microsoft's C# coding conventions:

```csharp
// Use PascalCase for public members
public class TaskManager
{
    public string TaskName { get; set; }
    
    public void AddTask(string description)
    {
        // Use camelCase for local variables
        var newTask = new Task(description);
        
        // Use meaningful names
        if (IsValidTask(newTask))
        {
            SaveToDatabase(newTask);
        }
    }
    
    // Use descriptive method names
    private bool IsValidTask(Task task)
    {
        return !string.IsNullOrWhiteSpace(task.Description);
    }
}
```

### Code Organization

- **One class per file**: Keep classes in separate files
- **Logical grouping**: Group related functionality together
- **Clear naming**: Use descriptive names for classes, methods, and variables
- **Comments**: Add comments for complex logic, but prefer self-documenting code

### Database Operations

```csharp
// Use proper disposal patterns
using (var connection = new SQLiteConnection(connectionString))
{
    connection.Open();
    // Database operations
}

// Use parameterized queries to prevent SQL injection
var command = new SQLiteCommand(
    "INSERT INTO tasks (description, completed) VALUES (@desc, @completed)", 
    connection);
command.Parameters.AddWithValue("@desc", description);
command.Parameters.AddWithValue("@completed", false);
```

## 🧪 Testing Guidelines

### Manual Testing

Before submitting a PR, test:

1. **Core functionality**: Ensure all main features work
2. **Edge cases**: Test with empty inputs, large datasets, etc.
3. **UI responsiveness**: Check that the interface remains responsive
4. **Database operations**: Verify data is saved and retrieved correctly

### Test Scenarios

- **Task Management**: Create, edit, delete, and complete tasks
- **Focus Sessions**: Start, stop, and save focus sessions
- **Mood Tracking**: Record and view mood entries
- **Statistics**: Verify charts and data display correctly
- **AI Chat**: Test chat functionality (if LM Studio is available)

## 🐛 Issue Reporting

### Before Reporting

1. **Search existing issues**: Check if the issue already exists
2. **Reproduce the issue**: Ensure you can consistently reproduce it
3. **Gather information**: Collect relevant details about your environment

### Issue Template

```markdown
## Bug Description
Clear description of the bug

## Steps to Reproduce
1. Step one
2. Step two
3. Step three

## Expected Behavior
What should happen

## Actual Behavior
What actually happens

## Environment
- OS: Windows 10/11
- .NET Framework version: 4.7.2
- Visual Studio version: 2022

## Screenshots
Add screenshots if applicable

## Additional Context
Any other relevant information
```

## 💡 Feature Requests

### Before Requesting

1. **Check existing requests**: Look for similar feature requests
2. **Consider the scope**: Ensure the feature fits the project's goals
3. **Think about implementation**: Consider how it might be implemented

### Feature Request Template

```markdown
## Feature Description
Clear description of the proposed feature

## Problem Statement
What problem does this feature solve?

## Proposed Solution
How should this feature work?

## Alternatives Considered
Other solutions you've considered

## Additional Context
Mockups, examples, or other relevant information
```

## 🏷️ Labels and Milestones

We use labels to categorize issues and PRs:

- **`bug`**: Something isn't working
- **`enhancement`**: New feature or request
- **`documentation`**: Improvements or additions to documentation
- **`good first issue`**: Good for newcomers
- **`help wanted`**: Extra attention is needed
- **`question`**: Further information is requested

## 🎉 Recognition

Contributors will be recognized in:

- **README.md**: Contributors section
- **Release notes**: Major contributions mentioned
- **GitHub**: Contributor graphs and statistics

## 📞 Getting Help

If you need help:

1. **Check documentation**: Review README and existing issues
2. **Ask questions**: Create an issue with the `question` label
3. **Join discussions**: Participate in issue discussions

## 📚 Resources

- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- [Windows Forms Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)
- [SQLite Documentation](https://www.sqlite.org/docs.html)
- [Git Handbook](https://guides.github.com/introduction/git-handbook/)

---

Thank you for contributing to MindTrack! Your efforts help make this project better for everyone. 🙏 