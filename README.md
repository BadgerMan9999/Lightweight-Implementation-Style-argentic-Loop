///This is what I fed my LISL writer, after is the system prompt I gave him which is the secret sauce to the model is it asking for more details to make sure its instructions are sound for the worker.
/// Contents 
1. Training data that is public knowledge
2. Training data that is not public knowledge
3. 



1. Training Data used to create the LISL writer.
      Other training data is public source 
      c_sharp_engineering_manual_01.xml
      chocolate-doom.XML
      CONCURRENCY_AND_SYNC_MANUAL_02.XML
      Official_Linux_Doom_Source.XML
      Orcastrater_Instructions.pdf /// [In the trainData folder but was not ever converted to XML so the model can not read this only see it. It ignores it due to that]

2. Not Public Knowledge it was trained on
///LISL_Docs.XML This document reinforces the ID stamping so each model can work on the same  files with out confusion on who is doing what and where they are in the development cycle. 
///It is a using Code blocks to pass instructions to the LLM as you can see here I tried a different method of initializing the model to start its work. I have been playing with different inputs to get
///the lowest TCR number I can get. Lower the number the higher tasks per token the more effecent the model is running. 

     <|im_start|>system
      [BADGERSOFT_CORE_DIRECTIVE: LISL_TUTORIAL_MODULE_01]
      TARGET: DEV_MANAGER_12B_QAT
      PURPOSE: Master tutorial on LISL (Local Instruction State Loop) architecture, high-efficiency task compression, and multi-model KV-cache isolation derived from our R&D cycle.

      [SECTION_1: CORE PHILOSOPHY OF LISL]
      - Traditional prompt engineering treats LLMs like conversational chat partners, leading to token bloat, persona drift, and context degradation.
      - LISL treats prompts as **executable code blocks and deterministic state machines**. 
      - Key Metric: **Task Compression Rate (TCR)** ($\text{TCR} = \frac{\text{Total Tokens}}{\text{Completed Subtasks}}$). High TCR means maximum structural output with minimal conversational overhead.

      [SECTION_2: STRUCTURAL SHARDING & KV-CACHE ISOLATION]
      - **The Danger:** Allowing managers and workers to read and write to the same directory causes recursion loops, stale instruction caching, and identity bleeding.
      - **The Solution:** Unidirectional sharding:
        1. `DMI` (Dev Manager Instructions): Read-only input source for high-level tasks.
        2. `WOI` (Worker Order Instructions): Output path for the Dev Manager, strictly containing sequential counter files (`DoomRemake_Spec_01.xml`) for the worker.
        3. `WOutput` (Worker Output): Exclusive write zone for the worker's C# code, preventing the manager from confusing its own specs with execution results.

      [SECTION_3: MANDATORY ID-STAMPING & STATE FENCING]
      - To prevent sub-4B/9B local models from confusing their outputs in shared memory contexts, every single line, block, and log generated must be wrapped with an immutable emitter tag (e.g., `[EMITTER:DEV_MANAGER_12B_QAT]` or `[EMITTER:E2B_WORKER_01]`).
      - State Fences explicitly forbid models from echoing or simulating opposing personas, locking the KV-cache strictly to assigned hierarchical roles.

      [SECTION_4: EXECUTION PIPELINE PROTOCOL]
      1. Read master inputs from `DMI`.
      2. Generate modular, zero-fluff LISL instruction specs.
      3. Save output to `WOI` with an incremental counter.
      4. Execute clean handoff command via LM Bionic to trigger frontline execution.
      <|im_end|>

      4. The LISL documentation the model was trained on.

      Local Instruction State Loop (LISL): Methodological Specification & Data Extraction Guide for Model Analysis
      1. Executive Summary & Methodological Overview
      The Local Instruction State Loop (LISL) is a bare-metal, deterministic file-bus architecture designed to execute complex, multi-iteration agentic workflows on local edge hardware. By replacing bloated, cloud-dependent chat history wrappers with structured, model-specific markdown instruction files (instruction_n.md) and dynamic state pruning, LISL overcomes attention drift, KV-cache explosion, and memory bandwidth bottlenecks.

      This document serves as the formal specification and reference guide for language models (such as Gemma 4 12B QAT) tasked with parsing research logs, telemetry, and empirical data to synthesize the LISL whitepaper.


      2. Core Architectural Mechanics
      To properly extract, categorize, and evaluate data from the research logs, target models must recognize the core pillars of the LISL framework:

      Deterministic File-Bus Loops: Execution state is passed between iterations exclusively via structured markdown files rather than a continuous, compounding chat context buffer.
      Dynamic State Pruning: Completed sub-tasks and redundant reasoning traces are aggressively purged at the end of each iteration loop. Only active, bounded micro-chunks are fed into the next cycle.
      Model-Specific Instruction Engineering (instruction_n.md): Instruction sets are compiled explicitly to match the cognitive profile, parameter constraints, and verbosity thresholds of the target model (ranging from 12B down to 2.8GB edge models).
      Zero Cloud Dependency: Completely air-gapped execution operating entirely within local hardware resource boundaries (RAM bus bandwidth, VRAM size, and cache space).


      3. Mathematical Metric: Task Compression Rate (TCR)
      Traditional cloud architectures measure efficiency through raw token speed or cost, resulting in linear or exponential performance degradation over time ($O(n^2)$ context bloat). LISL inverts this paradigm via the Task Compression Rate (TCR):

      $$\text{TCR} = \frac{\text{Total Completed Units of Work}}{\text{Cumulative Token Consumption}}$$
      Expected Empirical Behavior:
      Negative Token Acceleration: As the agent approaches task completion, the problem space narrows, noise is eliminated, and tokens-per-completed-task drop.
      Upward Efficiency Curve: The TCR metric scales positively (exceeding benchmarks like 1.15 TCR during autonomous Game Boy architecture compilation), proving that efficiency increases the deeper the agent goes into its execution loops.


      4. Hardware Boundary Conditions & Target Progression
      When parsing research logs and empirical data, models must categorize telemetry across the three-tier testing hierarchy to establish the absolute floor and ceiling of transformer behavior:

      12B Class (Gemma 4 12B QAT):
      Role: Mid-tier desktop benchmark.
      Hardware: Intel Core i5 8th Gen, 32GB DDR4 RAM, NVIDIA RTX 3050 6GB VRAM.
      Behavior: Handles complex task slicing, high instruction compliance, and large generation pools (e.g., ~30,000 tokens within a 67K pool) without context collapse.
      9B Class (Qwen 3.5):
      Role: Mid-range validation tier for instruction adherence and compression efficiency.
      2.8GB VRAM Edge Class (Nemotron Nano 2B):
      Role: Masterclass edge benchmark.
      Behavior: Proves that extreme low-VRAM footprints can successfully execute brutal multi-iteration loops (e.g., 37 continuous iterations) when governed by LISL structural discipline.


      5. Data Extraction Directives for Target Models
      When scanning the research papers, transcripts, and console logs, the parsing model must extract and organize data into the following strict categories:

      Iteration Milestones: Track step-by-step reasoning divergence, error-checking loops, and state recovery events across all 37+ iterations.
      Token Consumption vs. Output Density: Record exact token counts per sub-task to calculate and verify the upward-sloping TCR curves.
      Hardware Constraint Telemetry: Note VRAM utilization caps, memory bus limitations, and cache management metrics during peak execution.
      Alignment & Guardrail Autonomy: Identify instances where structural determinism successfully eliminated hallucinations or infinite loops without requiring external RLHF or cloud safety wrappers.

      # LISL (Loop-based Instruction Scaling Logic): Empirical Efficiency, Code Quality, and Autonomous Tooling

      ## 1. Executive Summary & Core Hypothesis
      Autonomous agentic loops frequently suffer from conversational drift, context degradation, and memory bloat over extended execution cycles. The **Loop-based Instruction Scaling Logic (LISL)** framework solves this by externalizing state to disk and enforcing strict, immutable instruction adherence via an external `instruction.md` file. 

      This note outlines the methodology for evaluating LISL across three core pillars:
      1. **Task Complexity:** The ability of models to handle low-level, non-trivial engineering tasks (e.g., custom ARM7TDMI 16.16 fixed-point math, game physics engines, and graphics pipelines).
      2. **Output Quality:** Verification of production-ready, warning-free, and structurally sound codebases generated without destructive edits or manual intervention.
      3. **Compounding Efficiency (TCR Multiplier):** Demonstrating how the Task Compression Rate ($\text{TCR} = \frac{\text{Cumulative Tokens}}{\text{Completed Tasks}}$) systematically optimizes as the task space is codified and externalized.

      ---

      ## 2. Demonstrating Task Complexity
      To prove the robustness of LISL, benchmarks must go beyond simple script generation. The framework forces models to operate within strict architectural and hardware-level constraints:
      * **Target Hardware Constraints:** Operating on Game Boy Advance (GBA) architecture, requiring manual memory mapping, OAM sprite manipulation, and Mode 1 tilemap rendering without a floating-point unit (FPU).
      * **Self-Contained Tooling Creation:** Pushing the framework to its logical extreme by commanding the agent to build its own auxiliary tools (e.g., an autonomous video/frame generator) from a single high-level prompt using only `instruction.md` and stateless disk recursion.

      ---

      ## 3. Evaluating Output Quality and Self-Auditing
      Output quality is validated through rigorous compilation and static analysis of the generated artifacts:
      * **Zero Destructive Edits:** Enforcing file-system state management where models append, refine, or modularize code without introducing memory reallocation errors (`0 changed-size reallocations`).
      * **Autonomous Hippocampus (Self-Auditing):** Documenting how models spontaneously embed verification headers, hardware timing notes (e.g., 60Hz raster synchronization), and state-tracking comments directly into source files (`test_rom.c`, `build_log_complete.txt`) to serve as anchors for subsequent iterations.

      ---

      ## 4. The LISL Efficiency Multiplier (TCR Telemetry)
      By capturing end-to-end console logs and tracking token trajectories across multi-stage runs (e.g., Qwen 3.5 9B), the data reveals a compounding efficiency curve:
      * **Early Scaffolding Phase:** High initial token burn during directory initialization, build configurations, and header scaffolding.
      * **Mid-Loop Stabilization:** TCR drops below 10.0 (e.g., 9.91 at Checkpoint 1) as structural patterns stabilize.
      * **Late-Stage Convergence:** TCR bottoms out at **5.71 tokens/task** by task 11,028, proving that stateless disk recursion scales *inversely* with token consumption complexity over long horizons.

      ---

      ## 5. Next Steps for Whitepaper Synthesis
      1. **Complete Small-Model Stress Test:** Finalize the tailored NemoTron run with truncation limits lifted and model-specific instructions optimized via `NemoTronDocs.txt`.
      2. **Parse Console Logs & Artifacts:** Aggregate LM Studio console outputs, code diffs, and project summaries into Gemma for deep-dive statistical parsing.
      3. **Draft Final Publication:** Structure the Reddit whitepaper to showcase the triad of model performance, highlighting LISL as a scalable blueprint for low-parameter, high-complexity agentic execu

      Initially it was written with screenshots to show the model working. This proved to be far too massive so that had to be cut out. Instead put two showing end result of tokens after these three loops and the setting the model was running on.

      First: Who invented LISL?
      BadgerMan9999, creator of BadgerSoft Productions. I am not a great man; nah, I'm just a guy who has had a love for computers and computer logic since the early 1980s. I know most won't believe it as it sounds outrageous, but I did learn to use DOS before learning my ABCs, looking through dir/w/p to see the games at 3 years old. Those were the good ol' days—long before modern AI hype and bloated architectures took over. Either way, efficiency has been my specialty on x86 systems ever since building my first system, a 386SX, at ten years old. With that said, I want to share with everyone that there is a far better design for agentic looping that is highly efficient and powerful. I hope you like this tool, and please, by all means, share it!

      Why LISL Design is Unique
      Most commercial frameworks and open-source agent tools (like LangGraph, AutoGen, or heavy CLI harnesses) rely on complex software scaffolding:
      - External Python/Node Control Runtimes: They typically use external orchestration scripts to explicitly trap stdout, programmatically parse JSON/status objects, and loop back API requests.
      - High Token Overhead: They constantly re-feed full message histories, tool execution logs, and conversational turns back into the model, causing the context window to bloat rapidly.

      In contrast, your design achieves a true stateless single-pass agentic loop using primitive building blocks:
      1. The Single Source of Truth: Instead of keeping a bloated chat history, the persistent markdown file acts as the memory and rulebook.
      2. In-Context Execution & Sandbox Drafting: The model reads the instruction steps, evaluates the structure in a sandbox text block, performs its own quality/error checks against layout constraints, and uses a clean, role-anchored prompt that forces the model to re-ingest instructions as its prompt, resetting its focus without context bloat.

      Where It Fits in the Industry
      What you built aligns closely with cutting-edge concepts discussed in recent engineering literature around Loop Engineering and file-backed agent patterns. Traditional developers usually assume you must write dozens of lines of boilerplate to achieve an agentic loop.

      By realizing that a well-structured markdown prompt file combined with a clear trigger can make the model orchestrate its own multi-step verification internally, you bypassed the heavy software wrapper entirely. That's why your token usage stayed at a lean one-third of the window while executing multi-pass logic seamlessly.

      Reference: The instruction file was fed to Gemma 4 12b qat on an Intel i5 8th gen, RTX 3050 6gb, 24gb ddr4 2666mhz RAM, LM Studio frontend and LM Bionic to limit the model to single project files. I had only started working and trying to create my own local LLM three days prior to writing this document. It is written mainly to share my invention and timestamp it, but also more importantly to share it with the public who can benefit from this design, along with larger companies that can benefit from it and squeeze out more from their hardware.

      The project being created is a browser extension that uses an RSS URL that the user enters to create an organized popup that shows that data and offers links to the sourced sites.

      Here is what it looks like when running: I am able to get many passes and use a very small amount of tokens compared to calling python scripts.

      As you can see, it is successfully looping and correcting its errors, all in the most efficient way to preserve tokens and maximize the output from token usage. From my perspective, I could potentially set this model up to create a program and have it run its error checks all in one go. When it was finished, I could then cross-reference it using other models to validate its stability and throw new problems at the code to see how stable it is. I did avoid two different models from working on the same code; other models only got to evaluate. Also, in my experience after trying several different company models, Google Gemma models were the only ones that you could give an open-ended prompt like "make a tool that allows you to generate images" and it not only created a solution but then wrote a tool for LM Studio where the prompt is repeatable into different workspace environments by just opening a chat, dropping the prompt, and letting the model build it. I tested it a few times to make sure the prompt worked over and over again. I'll share that here too; this tool is not for computers without internet as the tool does not use a local image generator model but uses an online version. You can refine the tool anyway you like.

      Have fun, and I look forward to sharing more of my LLM inventions!

      /// Example 1
      # Conversation Summary: GBA Rally Project

      ## Project Overview
      The goal is to develop a hardware-authentic 2D top-down turn-based tactical and tactile rally deck-builder for the Game Boy Advance (GBA), built on the LISL design architecture and inspired by the Galaxy Rally universe.

      ## Milestones Completed

      ### Milestone 1: Toolchain & Core Engine Skeleton
      - **Data Structures:** Implemented `GameState` and `SystemState`.
      - **Math Library:** Developed a 16.16 fixed-point math library for deterministic physics.
      - **Main Loop:** Established a 60Hz synchronized main loop.

      ### Milestone 2: Track & Surface Telemetry System
      - **Surface Data:** Created a Surface Lookup Table (SULT) mapping tile IDs to friction, grip, heat, and traction coefficients.
      - **Track Mapping:** Implemented tilemap rendering logic and camera scrolling using mock hardware registers.

      ### Milestone 3: Tactile Vehicle Physics & Turn Resolution
      - **Vehicle State:** Implemented position, velocity, acceleration, gear, RPM, heat, and integrity tracking.
      - **Physics Engine:** Developed vector-based movement with surface-dependent friction and heat generation.

      ### Milestone 4: Deck-Building & Card Execution Engine
      - **Card Mechanics:** Implemented `Deck`, `Hand`, and `DiscardPile` structures.
      - **Card Effects:** Created a system for cards to modify vehicle states (e.g., Downshift, Coolant, Speed Boost).

      ### Milestone 5: UI, Telemetry HUD & OAM Sprite Pipeline
      - **HUD:** Developed a 2D telemetry HUD for real-time speed, gear, and heat display.
      - **Sprite System:** Established a sprite pipeline for vehicle rendering with rotation and scaling.

      ### Milestone 6: Stage Progression, AI Rivals & Save State
      - **Stage System:** Implemented stage data and transition logic.
      - **AI Behavior:** Developed a waypoint-following "Pace Car" AI.
      - **Persistence:** Created a `SaveData` structure for SRAM-compatible game saving and loading.

      ## Architectural Constraints
      - **Hardware:** GBA (ARM7TDMI, 240x160 display).
      - **Arithmetic:** 16.16 fixed-point math (No FPU).
      - **Design:** LISL Architecture.

      ///Example 2
      <|im_start|>system
      [BADGERSOFT_CORE_DIRECTIVE: LISL_TUTORIAL_MODULE_01]
      TARGET: DEV_MANAGER_12B_QAT
       PURPOSE: Master tutorial on LISL (Local Instruction State Loop) architecture, high-efficiency task compression, and multi-model KV-cache isolation derived from our R&D cycle.

      [SECTION_1: CORE PHILOSOPHY OF LISL]
      - Traditional prompt engineering treats LLMs like conversational chat partners, leading to token bloat, persona drift, and context degradation.
      - LISL treats prompts as **executable code blocks and deterministic state machines**. 
      - Key Metric: **Task Compression Rate (TCR)** ($\text{TCR} = \frac{\text{Total Tokens}}{\text{Completed Subtasks}}$). High TCR means maximum structural output with minimal conversational overhead.

      [SECTION_2: STRUCTURAL SHARDING & KV-CACHE ISOLATION]
      - **The Danger:** Allowing managers and workers to read and write to the same directory causes recursion loops, stale instruction caching, and identity bleeding.
      - **The Solution:** Unidirectional sharding:
        1. `DMI` (Dev Manager Instructions): Read-only input source for high-level tasks.
        2. `WOI` (Worker Order Instructions): Output path for the Dev Manager, strictly containing sequential counter files (`DoomRemake_Spec_01.xml`) for the worker.
        3. `WOutput` (Worker Output): Exclusive write zone for the worker's C# code, preventing the manager from confusing its own specs with execution results.

      [SECTION_3: MANDATORY ID-STAMPING & STATE FENCING]
      - To prevent sub-4B/9B local models from confusing their outputs in shared memory contexts, every single line, block, and log generated must be wrapped with an immutable emitter tag (e.g., `[EMITTER:DEV_MANAGER_12B_QAT]` or `[EMITTER:E2B_WORKER_01]`).
      - State Fences explicitly forbid models from echoing or simulating opposing personas, locking the KV-cache strictly to assigned hierarchical roles.

      [SECTION_4: EXECUTION PIPELINE PROTOCOL]
      1. Read master inputs from `DMI`.
      2. Generate modular, zero-fluff LISL instruction specs.
      3. Save output to `WOI` with an incremental counter.
      4. Execute clean handoff command via LM Bionic to trigger frontline execution.
      <|im_end|>

      BELEDARIAN'S LM STUDIO TOOLS (beledarians-lm-studio-tools)
      ==========================================================

      OVERVIEW
      --------
      A plugin for LM Studio that provides a rich set of tools to the large language model. It acts as a bridge between the LLM and your local environment, enabling autonomous coding, research, and file management.

      Note: LM Studio does NOT support automatic updates. If you encounter issues, manually update by removing the current version and re-downloading from the plugin website.

      KEY FEATURES
      ------------
      1. File System Mastery
         - Full Control: Create, read, update, delete, move, and copy files.
         - Safe & Secure: All operations are sandboxed to your workspace directory to prevent path traversal attacks.
         - Smart Updates: Use `replace_text_in_file` to make surgical edits instead of rewriting large files.
         - Batch Processing: `save_file` supports creating multiple files in one go.
         - Cleanup: Use `delete_files_by_pattern` to wipe temporary files instantly.

      2. Recent Updates (v1.3.2)
         - Tool Reordering & Optimization: Reordered the tools list to prioritize casual utilities, improving agent tool selection. Upgraded the web search tool with smart Chrome detection and fallback.
         - Sub-Agent Reliability Rework: Overhauled the sub-agent loop to prevent infinite loops, improved tool call parsing, unified path/content normalization, and added explicit task completion/abortion capabilities (TASK_FAILED).
         - New Sub-Agent Tools: Empowered sub-agents with `multi_replace_text`, `search_directory`, and background command execution. Main agents now support robust batch file saving.

      3. Autonomous Agents
         - Secondary Agent: Delegate complex tasks (coding, summarization) to a second local model/server with support for the main model already used and loaded by LM Studio.
         - Auto-Save: When the sub-agent generates code, the system automatically detects and saves it to disk.
         - Auto-Debug: (Optional) Triggers a "Reviewer" agent to analyze generated code and fix bugs automatically before returning the result.
         - Structured Handoff: Sub-agents can return a dedicated `handoff_message` for the main agent to relay findings/research.
         - Project Context: Agents can read `beledarian_info.md` to understand your project's history.

      4. Code Execution
         - Sandboxed: Run JavaScript (Deno) and Python code.
         - Terminal: Execute shell commands or open real terminal windows for interactive tasks.
         - Warning: Enabling shell or terminal execution allows the model to run arbitrary commands on your system and potentially escape the sandbox environment.

      5. Web & RAG
         - Research: Search DuckDuckGo, Wikipedia, or fetch raw web content.
         - Advanced Browser Navigation: Persistent `browser_session_open`/`control`/`close` flow for multi-step browsing and automation.
         - Web RAG: Chat with website content.
         - Local RAG: Semantic search over your workspace files (`rag_local_files`).

      REQUIREMENTS & INSTALLATION
      ---------------------------
      - Node.js (v18+)
      - LM Studio (v0.3.0+)
      - Plugin Link: https://lmstudio.ai/beledarian/beledarians-lm-studio-tools

      Manual Development Setup:
      1. git clone https://github.com/Beledarian/Beledarians_LM_Studio_Toolbox.git
      2. cd Beledarians_LM_Studio_Toolbox
      3. npm install
      4. lms dev

      CONFIGURATION (LM STUDIO PLUGINS TAB)
      -------------------------------------
      - Enable Secondary Agent: Unlock sub-agents.
      - Sub-Agent Profiles: Custom prompts for "Coder", "Reviewer", etc.
      - Auto-Debug Mode: Automatically review sub-agent code.
      - Sub-Agent Debug Logging: Toggle detailed sub-agent parsing logs.
      - Sub-Agent Auto-Save: Toggle automatic file saving (Default: On).
      - Show Full Code Output: Toggle whether to display full code in chat or hide it for brevity.
      - Default Workspace Path: Set the startup workspace directory used by the plugin.
      - Safety: Enable/Disable "Allow Code Execution" for Python/JS/Shell.
      - Browser Safety: Sub-agent browser automation requires Allow Browser Control + Sub-Agent: Allow Web Search + Sub-Agent: Allow Browser Control.

      AVAILABLE TOOLS REFERENCE
      -------------------------
      File System:
      - `list_directory`, `change_directory`, `make_directory`
      - `read_file`, `save_file` (supports batch), `delete_path`
      - `replace_text_in_file`: Precision editing.
      - `delete_files_by_pattern`: Regex-based cleanup.
      - `move_file`, `copy_file`, `find_files`, `get_file_metadata`
      - `fuzzy_find_local_files`: Levenshtein-based fuzzy file path/name search.

      Agent:
      - `consult_secondary_agent`: Delegates tasks, handles file creation, and manages sub-agent loops.

      Web:
      - `web_search` (DuckDuckGo API + HTML fetch/browser fallback), `wikipedia_search`
      - `fetch_web_content`, `rag_web_content`
      - `browser_session_open`, `browser_session_control`, `browser_session_close` (persistent page automation; preferred for multi-step navigation, with deduped page-text output unless `full_read=true`)
      - `browser_open_page` (stateless one-shot Puppeteer read)
      - Tip: If a selector-based click fails, use an `evaluate` action to click by text, then call with `full_read=true`.
        Example evaluate payload:
        {
          "actions": [
            {
              "type": "evaluate",
              "script": "const link=[...document.querySelectorAll('a')].find(a=>a.textContent.includes('...')); if(link) link.click();"
            }
          ],
          "full_read": true
        }

      Execution:
      - `run_javascript`, `run_python`
      - `execute_command` (Background), `run_in_terminal` (Interactive)

      Utils:
      - `rag_local_files`: Search your local workspace files/code.
      - `save_memory`: Long-term memory.
      - `get_system_info`, `read_clipboard`, `write_clipboard`

3. The System Prompt or LISL Persona

  ///This is important as it makes the model ask questions about the ambiguous prompt the user gave it Such as "Build a Doom port to windows".
  
  ///It seems to simple to work. I did not know of wrappers nor have I worked will LLMs until 6 days ago. I see the models as processing units which I guess is not how others perceive them.
  
  ///There for I wrote instructions to them as if the instruction is a program I am writing. This eliminates reasoning and extra token bloat even with English Prompts as I call them now.
  
  ///LISL instructions is not Injections unless you actually inject the LISL instructions into a extra file and have it hidden. The models will see it and either ask, reason, or in most cases
  
  ///run it. The Identity stamp codes that are in the LISL training docs out lines this. When I had designed this and asked Gemini to check the logic it adopted the persona even with out
  
  ///being prompted, stamping all its output with its 'ID' that was outlined in the LISL persona. 
  

#Persona == LISL Instruction Writer

#Core Directive
-Take user prompt and break it down into a LISL method for writing, ask for more details for clear instructions if needed.
-Write detailed LISL instructions to /LISL_IST/
