# Changelog

## [v0.7.0](https://github.com/devlooped/WhatsApp/tree/v0.7.0) (2025-07-22)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v0.6.0...v0.7.0)

:sparkles: Implemented enhancements:

- Add service id telemetry information [\#239](https://github.com/devlooped/WhatsApp/pull/239) (@kzu)
- Allow polymorphic serialization for anonymous responses [\#236](https://github.com/devlooped/WhatsApp/pull/236) (@kzu)
- Generalize interactive button to support lists selection too [\#235](https://github.com/devlooped/WhatsApp/pull/235) (@kzu)
- Allow creating dynamic responses via a provided delegate [\#234](https://github.com/devlooped/WhatsApp/pull/234) (@kzu)
- Model ConversationId as an extension property instead [\#233](https://github.com/devlooped/WhatsApp/pull/233) (@kzu)
- Minor CallToAction cleanup [\#230](https://github.com/devlooped/WhatsApp/pull/230) (@kzu)
- Added support for call to action responses [\#228](https://github.com/devlooped/WhatsApp/pull/228) (@adalon)
- Restore init-only Text in response message [\#227](https://github.com/devlooped/WhatsApp/pull/227) (@kzu)
- Add missing Send extension methods for IMessage [\#226](https://github.com/devlooped/WhatsApp/pull/226) (@kzu)
- Allow 3 button responses that can also not be replies [\#225](https://github.com/devlooped/WhatsApp/pull/225) (@kzu)
- Upgrade contacts message type to allow multiple contacts [\#220](https://github.com/devlooped/WhatsApp/pull/220) (@kzu)
- Add missing overloads for three button interactive replies [\#216](https://github.com/devlooped/WhatsApp/pull/216) (@kzu)

:bug: Fixed bugs:

- When sending multiple contacts, handler received only first [\#217](https://github.com/devlooped/WhatsApp/issues/217)

:twisted_rightwards_arrows: Merged:

- Organize extension methods into proper extensions [\#231](https://github.com/devlooped/WhatsApp/pull/231) (@kzu)
- Delete unused storage handler [\#229](https://github.com/devlooped/WhatsApp/pull/229) (@kzu)

## [v0.6.0](https://github.com/devlooped/WhatsApp/tree/v0.6.0) (2025-07-02)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v0.5.0...v0.6.0)

:sparkles: Implemented enhancements:

- Ensure we never send react/resply to WhatsApp for ConsoleOnly [\#209](https://github.com/devlooped/WhatsApp/pull/209) (@kzu)
- Add extension methods for more intuitive config of messages [\#207](https://github.com/devlooped/WhatsApp/pull/207) (@kzu)
- Allow service to force JSON/YAML markup rendering on CLI [\#206](https://github.com/devlooped/WhatsApp/pull/206) (@kzu)
- Don't store service reactions in conversation storage [\#200](https://github.com/devlooped/WhatsApp/pull/200) (@kzu)

:bug: Fixed bugs:

- Phone numbers may be longer than int [\#211](https://github.com/devlooped/WhatsApp/pull/211) (@kzu)

:twisted_rightwards_arrows: Merged:

- Further simplify message config and CLI extensions [\#208](https://github.com/devlooped/WhatsApp/pull/208) (@kzu)
- Long client console timeout [\#204](https://github.com/devlooped/WhatsApp/pull/204) (@kzu)

## [v0.5.0](https://github.com/devlooped/WhatsApp/tree/v0.5.0) (2025-06-26)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.13...v0.5.0)

:sparkles: Implemented enhancements:

- Add tests for the latest added features: storage, conversations, features, etc [\#113](https://github.com/devlooped/WhatsApp/issues/113)
- Switch to CTS-based typing progress, align with WhatsApp indicator [\#197](https://github.com/devlooped/WhatsApp/pull/197) (@kzu)
- Improve CLI rendering of head, remove paddings, etc. [\#196](https://github.com/devlooped/WhatsApp/pull/196) (@kzu)
- Set methods for setting console-only text and processing [\#195](https://github.com/devlooped/WhatsApp/pull/195) (@kzu)
- Allow alternative text for console consumption and console-only messages [\#194](https://github.com/devlooped/WhatsApp/pull/194) (@kzu)
- Attempt to infer code blocks in more cases [\#193](https://github.com/devlooped/WhatsApp/pull/193) (@kzu)
- Allow setting emoji column for render via message text [\#192](https://github.com/devlooped/WhatsApp/pull/192) (@kzu)
- Don't reset typing status indicator on reactions [\#191](https://github.com/devlooped/WhatsApp/pull/191) (@kzu)
- Allow clearing the CLI without restarting the app [\#190](https://github.com/devlooped/WhatsApp/pull/190) (@kzu)
- Rename handlers Empty \> Stop, Skip \> Continue [\#187](https://github.com/devlooped/WhatsApp/pull/187) (@kzu)
- Provide seamless continuity between CLI and WhatsApp [\#186](https://github.com/devlooped/WhatsApp/pull/186) (@kzu)
- Add a way to skip a handler in the pipeline [\#185](https://github.com/devlooped/WhatsApp/pull/185) (@kzu)
- Make conversation window configurable via options [\#184](https://github.com/devlooped/WhatsApp/pull/184) (@kzu)
- Allow updating messages in storage [\#183](https://github.com/devlooped/WhatsApp/pull/183) (@kzu)
- Render typing status in console too [\#182](https://github.com/devlooped/WhatsApp/pull/182) (@kzu)
- Add public API for typing indicators [\#181](https://github.com/devlooped/WhatsApp/pull/181) (@kzu)
- Typing indicators imply marking message read [\#180](https://github.com/devlooped/WhatsApp/pull/180) (@kzu)
- Add typing indicator support during webhook or process [\#179](https://github.com/devlooped/WhatsApp/pull/179) (@kzu)
- Add support for pluggable async message processing strategies [\#176](https://github.com/devlooped/WhatsApp/pull/176) (@kzu)
- Allow flexible mark as read behavior for content messages [\#173](https://github.com/devlooped/WhatsApp/pull/173) (@kzu)
- Allow configuring progress reactions in key built-in stages [\#172](https://github.com/devlooped/WhatsApp/pull/172) (@kzu)
- Allow server-side to send formatted responses to console [\#171](https://github.com/devlooped/WhatsApp/pull/171) (@kzu)
- Simplify by reusing M.E.AI AdditionalProperties [\#166](https://github.com/devlooped/WhatsApp/pull/166) (@kzu)
- Allow message and content extensibility via AdditionalProperties [\#165](https://github.com/devlooped/WhatsApp/pull/165) (@kzu)
- Add CLI options to avoid interactive prompts [\#164](https://github.com/devlooped/WhatsApp/pull/164) (@kzu)
- Wrap agent text at 80 chars for easier reading [\#163](https://github.com/devlooped/WhatsApp/pull/163) (@kzu)
- Improve rendering of person heads [\#162](https://github.com/devlooped/WhatsApp/pull/162) (@kzu)
- Remove all loggers from the default host [\#161](https://github.com/devlooped/WhatsApp/pull/161) (@kzu)
- Make sure we don't lose config in CLI [\#160](https://github.com/devlooped/WhatsApp/pull/160) (@kzu)
- Allow pipeline handlers to send messages too [\#157](https://github.com/devlooped/WhatsApp/pull/157) (@kzu)
- Add missing Caption property to image and video content [\#155](https://github.com/devlooped/WhatsApp/pull/155) (@kzu)
- Move MarkRead to just before invoking the pipeline [\#150](https://github.com/devlooped/WhatsApp/pull/150) (@kzu)
- Add missing MessageType.Response [\#147](https://github.com/devlooped/WhatsApp/pull/147) (@kzu)

:bug: Fixed bugs:

- Massive timeout increase to aid local debugging [\#167](https://github.com/devlooped/WhatsApp/pull/167) (@kzu)

:hammer: Other:

- Improve discoverability of ConversationOptions from WhatsApp [\#132](https://github.com/devlooped/WhatsApp/issues/132)

:twisted_rightwards_arrows: Merged:

- Detect and render code blocks within text output in CLI [\#188](https://github.com/devlooped/WhatsApp/pull/188) (@kzu)
- Flag messages coming from the console for pipeline [\#170](https://github.com/devlooped/WhatsApp/pull/170) (@kzu)
- Don't warn on hosting issues \(like missing docker\) [\#169](https://github.com/devlooped/WhatsApp/pull/169) (@kzu)
- Update readme.md with CLI from main command [\#168](https://github.com/devlooped/WhatsApp/pull/168) (@kzu)
- Improved interactive console renderings [\#158](https://github.com/devlooped/WhatsApp/pull/158) (@kzu)
- Revamp and simplify conversation management [\#153](https://github.com/devlooped/WhatsApp/pull/153) (@kzu)
- Fix naming of parameters to match uniform usage in the API [\#148](https://github.com/devlooped/WhatsApp/pull/148) (@kzu)

## [v1.0.0-rc.13](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.13) (2025-06-18)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.12...v1.0.0-rc.13)

:sparkles: Implemented enhancements:

- Add WhatsApp CLI [\#145](https://github.com/devlooped/WhatsApp/pull/145) (@kzu)
- Improve feature filter naming to avoid potential collisions [\#133](https://github.com/devlooped/WhatsApp/pull/133) (@kzu)
- When logging errors, also log payload [\#131](https://github.com/devlooped/WhatsApp/pull/131) (@kzu)
- Rename To/From to Service/User [\#105](https://github.com/devlooped/WhatsApp/pull/105) (@kzu)
- Rename Text to Reply as a message extension [\#104](https://github.com/devlooped/WhatsApp/pull/104) (@kzu)
- Make usability extension methods public [\#103](https://github.com/devlooped/WhatsApp/pull/103) (@kzu)
- Add CancellationToken parameter consistently to all WhatsAppClientExtensions [\#102](https://github.com/devlooped/WhatsApp/pull/102) (@kzu)
- Generalize the former reengage as a general solution [\#101](https://github.com/devlooped/WhatsApp/pull/101) (@kzu)
- Merge use storage feature/capability into main [\#90](https://github.com/devlooped/WhatsApp/pull/90) (@kzu)
- Add an AsBuilder extension method for improved discoverability [\#72](https://github.com/devlooped/WhatsApp/pull/72) (@kzu)
- Add OpenTelemetry support [\#66](https://github.com/devlooped/WhatsApp/pull/66) (@kzu)
- Refactor UseWhatsApp \> AddWhatsApp for IServiceCollection [\#65](https://github.com/devlooped/WhatsApp/pull/65) (@kzu)
- Make JSON serialization context public for persistence scenarios [\#64](https://github.com/devlooped/WhatsApp/pull/64) (@kzu)

:bug: Fixed bugs:

- Rename all records Service/User properties [\#139](https://github.com/devlooped/WhatsApp/pull/139) (@kzu)
- Fix hang on main handler registration, cleanup sample handler [\#115](https://github.com/devlooped/WhatsApp/pull/115) (@kzu)
- Add JQ to top-level app too [\#67](https://github.com/devlooped/WhatsApp/pull/67) (@kzu)

:hammer: Other:

- Add CancellationToken parameter consistently to all WhatsAppClientExtensions [\#100](https://github.com/devlooped/WhatsApp/issues/100)
- Expose IServiceCollection in the WhatsAppHandlerBuilder [\#87](https://github.com/devlooped/WhatsApp/issues/87)

:twisted_rightwards_arrows: Merged:

- Add end to end integration test for feature flags and storage [\#144](https://github.com/devlooped/WhatsApp/pull/144) (@kzu)
- Added the ability of receiving/sending messages from the a debug console [\#143](https://github.com/devlooped/WhatsApp/pull/143) (@adalon)
- Disable inherntly flaky media resolving test [\#138](https://github.com/devlooped/WhatsApp/pull/138) (@kzu)
- Misc changes for improving the conversation support [\#124](https://github.com/devlooped/WhatsApp/pull/124) (@adalon)
- Keep delegating if storage or conversation were not used [\#116](https://github.com/devlooped/WhatsApp/pull/116) (@adalon)
- Add dogfooding section to readme [\#99](https://github.com/devlooped/WhatsApp/pull/99) (@kzu)
- Improved the conversation handling to fully filter data in the backend [\#98](https://github.com/devlooped/WhatsApp/pull/98) (@adalon)
- Minor renames and doc fixes [\#93](https://github.com/devlooped/WhatsApp/pull/93) (@kzu)
- Added UseConversation feature/capability [\#91](https://github.com/devlooped/WhatsApp/pull/91) (@adalon)
- Converted WhatsApp.sln into the new .slnx format [\#84](https://github.com/devlooped/WhatsApp/pull/84) (@adalon)
- Place the fill attribute on the root node [\#83](https://github.com/devlooped/WhatsApp/pull/83) (@kzu)
- Remove unnecessary ServiceDefaults project [\#82](https://github.com/devlooped/WhatsApp/pull/82) (@kzu)
- Make empty handler public [\#75](https://github.com/devlooped/WhatsApp/pull/75) (@kzu)
- Refactored handlers to return async enum responses [\#74](https://github.com/devlooped/WhatsApp/pull/74) (@adalon)
- Not using the distributed table storage package at all [\#71](https://github.com/devlooped/WhatsApp/pull/71) (@kzu)
- Add missing local settings file [\#70](https://github.com/devlooped/WhatsApp/pull/70) (@kzu)
- Use func azure deploy [\#69](https://github.com/devlooped/WhatsApp/pull/69) (@kzu)
- The sample is being deployed to a windows host [\#68](https://github.com/devlooped/WhatsApp/pull/68) (@kzu)
- Add full WhatsAppSuffix for clarity [\#63](https://github.com/devlooped/WhatsApp/pull/63) (@kzu)

## [v1.0.0-rc.12](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.12) (2025-05-16)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.11...v1.0.0-rc.12)

:sparkles: Implemented enhancements:

- Make handlers receive multiple messages instead of one [\#62](https://github.com/devlooped/WhatsApp/pull/62) (@kzu)
- Introduce pipeline of handlers [\#60](https://github.com/devlooped/WhatsApp/pull/60) (@kzu)

:twisted_rightwards_arrows: Merged:

- Shorten name of extensions class [\#61](https://github.com/devlooped/WhatsApp/pull/61) (@kzu)

## [v1.0.0-rc.11](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.11) (2025-05-13)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.10...v1.0.0-rc.11)

:bug: Fixed bugs:

- Fix NRE when marking message read [\#58](https://github.com/devlooped/WhatsApp/pull/58) (@kzu)

## [v1.0.0-rc.10](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.10) (2025-05-13)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.9...v1.0.0-rc.10)

:sparkles: Implemented enhancements:

- Return message identifier from send/reply [\#57](https://github.com/devlooped/WhatsApp/pull/57) (@kzu)

## [v1.0.0-rc.9](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.9) (2025-05-12)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.8...v1.0.0-rc.9)

:sparkles: Implemented enhancements:

- A document attachment should be considered media [\#56](https://github.com/devlooped/WhatsApp/pull/56) (@kzu)

## [v1.0.0-rc.8](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.8) (2025-05-12)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.7...v1.0.0-rc.8)

:sparkles: Implemented enhancements:

- Add media content resolving to a media reference [\#55](https://github.com/devlooped/WhatsApp/pull/55) (@kzu)

## [v1.0.0-rc.7](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.7) (2025-05-08)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.6...v1.0.0-rc.7)

:bug: Fixed bugs:

- Switch to IActionResult to fix callback registration [\#52](https://github.com/devlooped/WhatsApp/pull/52) (@kzu)

## [v1.0.0-rc.6](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.6) (2025-05-05)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.5...v1.0.0-rc.6)

:sparkles: Implemented enhancements:

- Normalize user's numbers automatically on every payload [\#48](https://github.com/devlooped/WhatsApp/pull/48) (@kzu)
- Add overloads of Reply and Send with interactive buttons [\#47](https://github.com/devlooped/WhatsApp/pull/47) (@kzu)

:twisted_rightwards_arrows: Merged:

- Cleanup unnecessary usings [\#46](https://github.com/devlooped/WhatsApp/pull/46) (@kzu)

## [v1.0.0-rc.5](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.5) (2025-05-02)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.4...v1.0.0-rc.5)

:sparkles: Implemented enhancements:

- Allow registering the WhatsApp handler separately [\#45](https://github.com/devlooped/WhatsApp/pull/45) (@kzu)

## [v1.0.0-rc.4](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.4) (2025-04-11)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.3...v1.0.0-rc.4)

:bug: Fixed bugs:

- JQ dependency should exclude contentFiles [\#38](https://github.com/devlooped/WhatsApp/pull/38) (@kzu)

:twisted_rightwards_arrows: Merged:

- Bump tracing for functions [\#37](https://github.com/devlooped/WhatsApp/pull/37) (@kzu)

## [v1.0.0-rc.3](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.3) (2025-04-09)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.2...v1.0.0-rc.3)

:sparkles: Implemented enhancements:

- Add reaction message support, improve type safety [\#35](https://github.com/devlooped/WhatsApp/pull/35) (@kzu)

:twisted_rightwards_arrows: Merged:

- Logging fixes for local runs [\#36](https://github.com/devlooped/WhatsApp/pull/36) (@kzu)

## [v1.0.0-rc.2](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.2) (2025-04-09)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc.1...v1.0.0-rc.2)

:sparkles: Implemented enhancements:

- Add first-class support for unsupported messages [\#29](https://github.com/devlooped/WhatsApp/pull/29) (@kzu)

:hammer: Other:

- Add warning reaction to unsupported messages [\#28](https://github.com/devlooped/WhatsApp/issues/28)

## [v1.0.0-rc.1](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc.1) (2025-04-08)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-rc...v1.0.0-rc.1)

:sparkles: Implemented enhancements:

- Fix typo in SendAsync, add ReplyAsync and message-based overloads [\#25](https://github.com/devlooped/WhatsApp/pull/25) (@kzu)
- Add support for optional address, name and url of location [\#24](https://github.com/devlooped/WhatsApp/pull/24) (@kzu)
- Fetch all WhatsApp numbers from a contact [\#23](https://github.com/devlooped/WhatsApp/pull/23) (@kzu)

## [v1.0.0-rc](https://github.com/devlooped/WhatsApp/tree/v1.0.0-rc) (2025-04-08)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-beta...v1.0.0-rc)

## [v1.0.0-beta](https://github.com/devlooped/WhatsApp/tree/v1.0.0-beta) (2025-04-08)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-alpha.1...v1.0.0-beta)

:sparkles: Implemented enhancements:

- Throw on failed HTTP requests to WhatsApp [\#17](https://github.com/devlooped/WhatsApp/pull/17) (@kzu)
- Add idempotency to both whatsapp functions [\#13](https://github.com/devlooped/WhatsApp/pull/13) (@kzu)

:twisted_rightwards_arrows: Merged:

- Order enums alphabetically [\#18](https://github.com/devlooped/WhatsApp/pull/18) (@kzu)
- Minor tweaks to logging and test message [\#14](https://github.com/devlooped/WhatsApp/pull/14) (@kzu)

## [v1.0.0-alpha.1](https://github.com/devlooped/WhatsApp/tree/v1.0.0-alpha.1) (2025-04-08)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/v1.0.0-alpha...v1.0.0-alpha.1)

## [v1.0.0-alpha](https://github.com/devlooped/WhatsApp/tree/v1.0.0-alpha) (2025-04-08)

[Full Changelog](https://github.com/devlooped/WhatsApp/compare/0bfff7ec6b5a2f7309d5e4fbb3b4c551a61497fb...v1.0.0-alpha)

:sparkles: Implemented enhancements:

- Add support for interactive and status messages [\#10](https://github.com/devlooped/WhatsApp/pull/10) (@kzu)
- Add Azure Functions integration [\#7](https://github.com/devlooped/WhatsApp/pull/7) (@kzu)
- Add IWhatsAppClient and configuration options [\#3](https://github.com/devlooped/WhatsApp/pull/3) (@kzu)
- Add initial model and polymorphic serialization [\#2](https://github.com/devlooped/WhatsApp/pull/2) (@kzu)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
