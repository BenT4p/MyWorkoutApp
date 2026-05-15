MyWorkoutApp is a high-performance, cross-platform fitness tracking solution built with .NET MAUI. Designed for both Android and Windows, the application provides a seamless, localized experience for tracking strength training, visualizing progress, and managing personal fitness data.

🛠️ Tech Stack
Framework: .NET MAUI (.NET 8).

Language: C# & XAML.

Data Persistence: Local JSON-based storage utilizing FileSystem.AppDataDirectory.

UI/UX: Custom Graphics with GraphicsView, smooth animations, and adaptive icons.

AI-Assisted Development: Leveraged LLMs (Gemini & Claude) as pair programming partners for code reviews, logic optimization, and rapid prototyping.

✨ Key Features
Workout Management: Create and edit personalized workout templates with real-time logging for sets, reps, and weights.

Data Visualization: Interactive charts for progress tracking, implemented via IDrawable for high-performance rendering.

Smart Calculators: Dynamic Barbell Plate Calculator and a formula-based 1RM (One Rep Max) estimator.

Personalization: A context-aware greeting system and dynamic tips tailored to the user's gender, fitness goals, and time of day.

RTL Support: Fully localized Hebrew interface optimized for seamless mobile navigation.

🚀 Technical Challenges & AI Collaboration
AI Pair Programming: Collaborated with AI models (Gemini & Claude) to design complex algorithms, resolve RTL-specific UI bugs, and optimize the data store architecture.

Performance Optimization: Implemented App Shell preloading during the Splash Screen phase to eliminate UI "stuttering" and ensure a smooth entry into the dashboard.

Complex Logic: Developed a custom-built charting engine that supports user interaction (Tap Handling) and dynamic directional shifts for RTL support.
