# Changelog

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
