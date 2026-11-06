# Recovering a certificate password

## Open the app

On Windows 10/11, install the .NET 10 SDK to build, or the .NET 10 Desktop Runtime
to run an existing build.

1. Double-click `Build-and-Run.bat`.
2. Choose **5** to build and open the multithreaded version.
3. After building, you can use `Quick-Launch.bat`, option **2**.

## 1. Choose your certificate

Click **Browse files…** and select your `.pfx` or `.p12` file.
The app reads it into RAM once per search. It does not upload or modify the file.

## 2. Choose how to search

### Try passwords I remember

This is the best place to start when a password might be long. Enter possible
passwords, company names, product names, or other words you remember, one per line.

With **Also try capitals…** enabled, the app tries:

- Each exact entry, lowercase, uppercase, and a capitalized first letter.
- Those forms followed by a number from 0 to 99.
- Those forms followed by a year from 1990 through the current year.
- Those forms followed by `!`, `123`, or `1234`.

These are simple variations, not every possible combination of your clues.
For example, to try a two-word phrase, enter the phrase as its own line. Spaces
are preserved. To test only exact passwords, turn the variations checkbox off.
Your entered clues are not saved.

### Search by length and characters

Use this when you know the unknown part is short. Choose numbers, lowercase
letters, letters and numbers, or custom characters. For custom characters, type
them directly: `abc123!` — do not add separating commas. A comma or space you
type is treated as a possible password character.

Choose the shortest and longest possible lengths. All lengths in the range are
searched, shortest first. Character positions are not trimmed.

The app shows how many combinations this creates. Large ranges can take far
longer than a lifetime. A maximum of 31 characters does not make a 31-character
password practical to find by exhaustive search; remembered words and patterns
are much more useful.

### More options

- **Known beginning / ending:** Add these around every guess. In length mode,
  the length range refers only to the unknown middle.
- **CPU workers:** The multithreaded app defaults to half the logical processors
  to leave some capacity for other work. More workers may be faster but can make
  the computer less responsive. The original app defaults to one worker.
- **Save a log:** Off by default. Logs can contain attempted and recovered
  passwords. If enabled, select where to save them. Every-guess logging is
  slower and can produce very large files.

GPU acceleration is not included in this version.

## 3. Start and follow progress

Click **Start recovery**. The panel shows completed guesses, average speed,
elapsed time, and an estimate for checking the remaining search space.
The estimate is not a promise of when a password will be found.

- **Password found:** Use **Show password** or **Copy password**.
- **No match in this search:** Try different clues or settings. This means only
  that the selected guesses did not work.
- **Stop:** Waits for active checks to finish, flushes any enabled log, and
  unlocks the settings. Starting again begins a new search; there is no saved
  resume point. Closing the window also stops the run safely.

Logs use a bounded buffer. They flush on a successful match, at most about a
second between active log writes, and when a run ends. A crash or power loss can
lose recent buffered entries. A log failure does not stop recovery; the app
reports it after the run.

## Troubleshooting

- **Check your settings:** Read the explanation in the progress panel. Select a
  file and supply at least one clue, or a valid length range and character set.
- **Could not complete the search:** Check the file is a valid PFX/P12, readable,
  and within the .NET certificate loader's limits.
- **Slow search:** Turn off every-guess logging, close competing CPU-heavy jobs,
  and narrow the search. An unfamiliar PFX may use a much more expensive password
  derivation function than another PFX.
- **Executable not found:** Build first. Executables are in each project's
  `bin\Release\net10.0-windows` directory.

See CONTRIBUTING.md for tests and PERFORMANCE.md for benchmark details.
