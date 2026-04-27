# Sands of the Unseen — Team Repository Rules & Setup Guide

This document covers everything you need to work on this project with Git and GitHub.
Read it fully before touching the repository. When in doubt, ask Meshari before doing anything you are not sure about.

---

## How This Repository Is Structured

```
main        ← The stable, always-playable version. Never commit here directly.
  └── develop  ← The shared working branch. All features merge here first.
        ├── feature/your-task-name   ← Your personal branch for each task.
        ├── feature/another-task
        └── art/character-textures
```

**The only branch you will ever create or push to is your own feature branch.**
You will never commit directly to `develop` or `main`.

---

---

# PART 1 — PREREQUISITES

Install everything in the order listed. Do not skip steps.

---

## Windows — Installing Prerequisites

### Step 1 — Install Git

1. Go to https://git-scm.com/download/win
2. Download the installer and run it.
3. During installation, accept all the default options.
4. When asked about the default editor, choose **Visual Studio Code** if you have it, otherwise leave as default.
5. When installation is done, open **Git Bash** from the Start Menu and run:
   ```
   git --version
   ```
   You should see something like `git version 2.x.x`. If you do, Git is installed correctly.

### Step 2 — Install Git LFS

Git LFS handles large files like textures, audio, and 3D models.

1. Go to https://git-lfs.com
2. Download the Windows installer and run it.
3. After installation, open **Git Bash** and run this command once:
   ```
   git lfs install
   ```
   You should see: `Git LFS initialized.`

### Step 3 — Confirm Unity Version

You must use the **exact same Unity version as the team: `6000.3.10f1`**.

1. Open **Unity Hub**.
2. Go to **Installs** and check if `6000.3.10f1` is listed.
3. If it is not, click **Install Editor**, find version `6000.3.10f1`, and install it.

Do not open the project with a different version. Unity will silently upgrade project files and break things for everyone else.

---

## Mac — Installing Prerequisites

### Step 1 — Install Git

Mac may already have Git installed. Check by opening **Terminal** and running:
```
git --version
```

If it shows a version, Git is already installed. If not, install it through Homebrew:

1. Install Homebrew first (if you don't have it). Open Terminal and run:
   ```
   /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
   ```
2. Then install Git:
   ```
   brew install git
   ```
3. Confirm it worked:
   ```
   git --version
   ```

### Step 2 — Install Git LFS

1. In Terminal, run:
   ```
   brew install git-lfs
   ```
2. Then run this once:
   ```
   git lfs install
   ```
   You should see: `Git LFS initialized.`

### Step 3 — Confirm Unity Version

Same as Windows. Open Unity Hub, go to Installs, and make sure you have `6000.3.10f1` installed.

---

---

# PART 2 — FIRST-TIME MACHINE CONFIGURATION

Do this after you have cloned the repository (see Part 3 first, then come back here).
This sets up Unity Smart Merge on your machine so Git can intelligently merge scene and prefab files.

---

## Configure Unity Smart Merge — Windows

Open **Git Bash inside the project folder** and run these three commands exactly as written:

```bash
git config --local merge.unityyamlmerge.name "Unity Smart Merge"
```

```bash
git config --local merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.3.10f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %P"
```

```bash
git config --local merge.unityyamlmerge.recursive binary
```

**Important:** The path above must match where Unity is installed on your machine.
To find it: open File Explorer, go to `C:\Program Files\Unity\Hub\Editor\` and check the folder name for your version. Replace `6000.3.10f1` in the commands if your folder name is different.

## Configure Unity Smart Merge — Mac

Open **Terminal inside the project folder** and run:

```bash
git config --local merge.unityyamlmerge.name "Unity Smart Merge"
```

```bash
git config --local merge.unityyamlmerge.driver "'/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Tools/UnityYAMLMerge' merge -p %O %B %A %P"
```

```bash
git config --local merge.unityyamlmerge.recursive binary
```

**Important:** If Unity is not in `/Applications`, find where Unity Hub installed it and update the path accordingly.

## Verify the Configuration Worked

Run this to confirm all three settings were saved:

```bash
git config --local --list | grep unityyamlmerge
```

You should see three lines appear. If you see nothing, the commands did not run correctly — try again or ask Meshari.

---

---

# PART 3 — CLONING THE REPOSITORY

Do this once when you first join the project.

---

### Step 1 — Get the Repository URL

Go to the GitHub repository page and click the green **Code** button. Copy the HTTPS URL.

### Step 2 — Open Git Bash (Windows) or Terminal (Mac)

Navigate to the folder where you want to store the project. For example:

```bash
cd /c/Unity
```

On Mac:
```bash
cd ~/Documents/Unity
```

### Step 3 — Clone the Repository

```bash
git clone PASTE-THE-URL-HERE
```

This downloads the entire project to your machine. Wait for it to finish.

### Step 4 — Enter the Project Folder

```bash
cd sands-of-the-unseen
```

### Step 5 — Download LFS Files

The clone command only downloads text pointers to large files. Run this to download the actual textures, audio, and models:

```bash
git lfs pull
```

This may take a few minutes depending on how many assets are in the project.

### Step 6 — Configure Smart Merge

Now go back to **Part 2** and run the Smart Merge configuration commands. You must do this inside the project folder.

### Step 7 — Open the Project in Unity

Open Unity Hub, click **Open**, and navigate to the cloned folder. Make sure Unity Hub selects version `6000.3.10f1` when opening it.

---

---

# PART 4 — DAILY WORKFLOW: STARTING YOUR DAY

Every time you sit down to work, do this before anything else.

---

### Step 1 — Switch to the develop branch

```bash
git checkout develop
```

### Step 2 — Pull the latest changes from GitHub

```bash
git pull origin develop
```

This downloads everything your teammates pushed since the last time you pulled. Always do this before starting new work.

### Step 3 — Download any new LFS files

```bash
git lfs pull
```

Run this after pulling if your teammates added new textures, audio, or models.

### Step 4 — Create or switch to your feature branch

If you are starting a new task, create a new branch (see Part 5).
If you are continuing work from yesterday, switch back to your branch:

```bash
git checkout feature/your-branch-name
```

---

---

# PART 5 — CREATING A FEATURE BRANCH

Every task gets its own branch. Never work directly on `develop`.

---

### Step 1 — Make sure you are on develop and it is up to date

```bash
git checkout develop
git pull origin develop
```

### Step 2 — Create your branch

```bash
git checkout -b feature/your-task-name
```

Replace `your-task-name` with a short description of what you are working on.

**Branch naming examples:**
```
feature/enemy-spawner
feature/upgrade-menu
feature/player-dash-animation
art/character-textures
art/sand-shader
fix/enemy-ai-bug
```

Rules for naming:
- All lowercase, words separated by dashes.
- Start with `feature/` for new gameplay or code work.
- Start with `art/` for visual assets.
- Start with `fix/` for bug fixes.
- Keep it short but descriptive.

### Step 3 — Confirm you are on the new branch

```bash
git branch
```

The branch with a `*` next to it is the one you are currently on.

---

---

# PART 6 — COMMITTING YOUR WORK

A commit is a saved snapshot of your changes. Commit often — small commits are easier to review and easier to undo if something goes wrong.

---

### Step 1 — Check what files you changed

```bash
git status
```

This shows all modified, added, or deleted files. Read through it before doing anything.

### Step 2 — Stage only the files you want to commit

Never use `git add .` or `git add -A` — these stage everything including files you did not intend to commit.

Stage specific files:
```bash
git add Assets/Scripts/EnemyAI.cs
git add Assets/Prefabs/Enemy.prefab
```

You can stage a whole folder:
```bash
git add Assets/Scripts/
```

### Step 3 — Write a commit message and commit

```bash
git commit -m "Add basic patrol movement to EnemyAI"
```

**Good commit messages:**
```
Add patrol waypoints to EnemyAI
Fix bullet not despawning after hitting wall
Increase player dash speed from 10 to 15
Add sand shader base texture
```

**Bad commit messages:**
```
stuff
fix
update
asdfgh
working now
```

Write what changed and why it changed. Future you will thank present you.

### Step 4 — Repeat as you work

You do not need to finish your entire task before committing. Commit every time you reach a small milestone or before stopping for the day.

---

---

# PART 7 — PUSHING YOUR WORK TO GITHUB

Pushing uploads your commits from your machine to GitHub so others can see them and you have a backup.

---

### First push of a new branch

When you push a branch for the first time, you need to tell GitHub where to put it:

```bash
git push -u origin feature/your-branch-name
```

The `-u` flag links your local branch to GitHub. You only need it the first time.

### All subsequent pushes

After the first push, just run:

```bash
git push
```

**When to push:**
- At the end of every work session.
- Before asking someone to review your work.
- Whenever you want a backup on GitHub.

---

---

# PART 8 — CREATING A PULL REQUEST (PR)

A Pull Request is how your work gets reviewed and merged into `develop`. You do this on the GitHub website, not in the terminal.

---

### Step 1 — Push your branch

Make sure all your commits are pushed (see Part 7).

### Step 2 — Go to GitHub

Open the repository page in your browser.

### Step 3 — Open a Pull Request

GitHub usually shows a yellow banner at the top saying your branch was recently pushed with a **"Compare & pull request"** button. Click it.

If the banner is gone, click the **"Pull requests"** tab, then click **"New pull request"**.

### Step 4 — Set the correct target

Make sure the PR is set to merge into **`develop`**, not `main`.

```
base: develop  ←  compare: feature/your-branch-name
```

### Step 5 — Write a description

Fill in the title and description:
- **Title:** Short summary of what the PR does (same style as a commit message).
- **Description:** Briefly explain what changed and anything the reviewer should know.

Example:
```
Title: Add enemy patrol movement

Description:
- EnemyAI now patrols between two waypoints
- Waypoints are set in the Inspector as serialized fields
- Does not affect shooting behavior yet
```

### Step 6 — Assign a reviewer

On the right side, under **Reviewers**, assign **Meshari** (or whoever is the version control lead this sprint).

### Step 7 — Submit

Click **"Create pull request"**.

You are done. Wait for the review. Do not merge your own PR.

---

---

# PART 9 — REVIEWING A PULL REQUEST

If Meshari assigns you as a reviewer, here is what to do.

---

### Step 1 — Read the PR description

Understand what was changed and why before looking at any code.

### Step 2 — Look at the Files Changed tab

GitHub shows a diff of every file that changed. Read through the changes.

Check for:
- Does the code make sense?
- Are there any obvious bugs or things that seem wrong?
- Does it match what the PR description says?
- Does the game still seem like it would run?

### Step 3 — Leave comments if needed

Click the `+` icon on any line to leave a comment. Be specific and constructive.

### Step 4 — Approve or Request Changes

Click **"Review changes"** (top right of the Files Changed tab):
- Click **Approve** if everything looks good.
- Click **Request changes** if something needs to be fixed first, and explain what.

### Step 5 — Do not click Merge

Only Meshari merges PRs into `develop`.

---

---

# PART 10 — KEEPING YOUR BRANCH UP TO DATE

If your teammates merge their work into `develop` while you are still working on your branch, your branch will fall behind. Update it regularly to avoid a large conflict later.

---

### Step 1 — Commit or stash your current work first

If you have uncommitted changes you are not ready to commit yet:
```bash
git stash
```
This temporarily saves your changes out of the way.

### Step 2 — Pull the latest develop

```bash
git checkout develop
git pull origin develop
git checkout feature/your-branch-name
```

### Step 3 — Merge develop into your branch

```bash
git merge develop
```

This brings your branch up to date with what everyone else has done.

If there are conflicts, see Part 11.

### Step 4 — Restore stashed changes (if you stashed)

```bash
git stash pop
```

**Do this at least every 1–2 days** to keep your branch close to `develop`. The longer you wait, the larger the conflict when it eventually comes.

---

---

# PART 11 — MERGE CONFLICTS

A merge conflict happens when two people changed the same part of the same file and Git does not know which version to keep. This is normal and fixable.

---

### When does this happen?

- Two people edited the same line of a script.
- Two people edited the same scene or prefab without coordination.

### Step 1 — Do not panic

A conflict does not delete your work. It just pauses the merge and asks you to decide.

### Step 2 — See which files have conflicts

```bash
git status
```

Files with conflicts are listed under **"Unmerged paths"**.

### Step 3 — For scripts (.cs files) — resolve in VS Code

Open the file in VS Code. Conflict markers look like this:

```
<<<<<<< HEAD (your version)
int speed = 10;
=======
int speed = 15;
>>>>>>> develop (incoming version)
```

You need to decide: keep your version, keep the incoming version, or combine them.
Delete the conflict markers and leave only the correct final code.

### Step 4 — For scenes and prefabs — use Smart Merge

Run:
```bash
git mergetool
```

Smart Merge will attempt to resolve the conflict automatically. If it cannot, it will open VS Code so you can resolve it manually.

If you are unsure what the correct version should be — **stop and ask Meshari**. Do not guess on scenes or prefabs.

### Step 5 — After resolving all conflicts

Stage the resolved files:
```bash
git add Assets/Scripts/ResolvedFile.cs
```

Then complete the merge:
```bash
git commit
```

---

---

# PART 12 — GIT LFS: LARGE FILES

Git LFS stores textures, audio, models, and other binary files separately from the regular Git history. You do not need to think about it much — the setup handles it automatically. But here is what you need to know.

---

### LFS happens automatically when you add files

If you add a `.png`, `.fbx`, `.mp3`, or any other binary asset and then commit it, LFS will handle it automatically because of the `.gitattributes` file in the repo. You do not need to run any special command.

### After pulling, always run:

```bash
git lfs pull
```

A regular `git pull` only downloads the LFS pointers (small text files), not the actual assets. Run `git lfs pull` to get the real files.

### Never drag binary files into a commit manually outside of the project

If you have a texture or model, always import it through the Unity Editor into the `Assets/` folder first. Then commit it through Git. This ensures LFS handles it correctly.

### Check LFS is working

If an image or model shows up as a small text file when you open it, LFS did not download the real file. Run `git lfs pull` to fix it.

---

---

# PART 13 — SCENE AND PREFAB RULES

Scenes and prefabs are the highest risk for conflicts. Follow these rules without exception.

---

### One person per scene at a time

Before opening a scene to edit it, announce in the team chat:

> "I am working in the Arena scene today."

No one else opens that scene until you are done and have pushed your changes. This is the most important rule on the team.

### One person per prefab at a time

Same rule applies to prefabs. If you need to change an enemy prefab that someone else created, talk to them first.

### If you need to edit the same scene as someone else

Split the work differently. For example, if two people need to work on the arena at the same time, one person works on gameplay objects and the other works in a separate UI scene. Never edit the same scene simultaneously.

### Never add objects to a scene without telling the team

A surprise scene edit from a teammate's branch can cause a conflict that is very hard to resolve.

---

---

# PART 14 — TEAM RULES SUMMARY

Print this and keep it visible.

```
1.  Never commit directly to develop or main. Always use a feature branch.

2.  Pull from develop every morning before starting work.

3.  Commit small and often. Write real commit messages.

4.  Claim your scene or prefab in team chat before opening it.

5.  Never edit the same scene as someone else at the same time.

6.  Run the game and make sure it launches before opening a Pull Request.

7.  Keep branches short. Merge within 2-3 days. Long branches = big conflicts.

8.  Pull before you push. Always.

9.  Never use: git push --force. Ever.

10. If something breaks and you do not know how to fix it, say so immediately.
    Do not spend hours hiding it. Ask Meshari.

11. Run git lfs pull after every pull.

12. Do not open the project with a Unity version other than 6000.3.10f1.
```

---

---

# PART 15 — QUICK COMMAND REFERENCE

```bash
# ── DAILY START ──────────────────────────────────────────────────────────
git checkout develop               # Switch to develop
git pull origin develop            # Download latest changes
git lfs pull                       # Download latest large files

# ── START A TASK ─────────────────────────────────────────────────────────
git checkout -b feature/task-name  # Create and switch to a new branch
git branch                         # Confirm which branch you are on

# ── SAVE YOUR WORK (COMMIT) ───────────────────────────────────────────────
git status                         # See what files changed
git add Assets/Path/To/File.cs     # Stage a specific file
git commit -m "Short description"  # Save a commit with a message

# ── UPLOAD TO GITHUB (PUSH) ───────────────────────────────────────────────
git push -u origin feature/name    # First push of a new branch
git push                           # All later pushes on the same branch

# ── UPDATE YOUR BRANCH FROM DEVELOP ──────────────────────────────────────
git checkout develop               # Switch to develop
git pull origin develop            # Download latest
git checkout feature/your-branch   # Switch back to your branch
git merge develop                  # Merge latest develop into your branch

# ── RESOLVE CONFLICTS ─────────────────────────────────────────────────────
git status                         # See which files have conflicts
git mergetool                      # Launch Smart Merge for scenes/prefabs
git add Assets/Fixed/File.cs       # Stage resolved file
git commit                         # Complete the merge

# ── LFS ───────────────────────────────────────────────────────────────────
git lfs pull                       # Download real binary files after a pull
git lfs ls-files                   # See which files are tracked by LFS

# ── SWITCH BRANCHES ───────────────────────────────────────────────────────
git checkout develop               # Switch to develop
git checkout feature/branch-name   # Switch to a feature branch
git branch                         # List all branches and see which you are on

# ── UNDO (SAFE) ───────────────────────────────────────────────────────────
git stash                          # Temporarily save uncommitted changes
git stash pop                      # Restore stashed changes
git checkout -- Assets/File.cs     # Discard changes to a single file (cannot undo)
```

---

*Last updated by Meshari — version control lead.*
*Questions? Ask before doing something you are not sure about.*
