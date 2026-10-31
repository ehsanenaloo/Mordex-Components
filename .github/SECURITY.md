# Security Policy

## Supported versions

Security fixes are made to the latest release, published as the `Mordex.Windows` package and tagged in this repository. Older versions are not patched. Please update.

## Reporting a vulnerability

Please **do not open a public issue** for a security problem.

Report it privately with GitHub's private vulnerability reporting:

1. Go to the repository's **Security** tab.
2. Choose **Report a vulnerability**.
3. Describe the problem and how to reproduce it.

Direct link: <https://github.com/ehsanenaloo/Mordex-Components/security/advisories/new>

Helpful details:

- the package version, and your .NET and Windows versions,
- the affected control or the designer,
- clear reproduction steps, ideally with a small project,
- the impact you believe it has.

Please do not include secrets, personal data or real customer data in a report.

## What to expect

This is a volunteer-maintained project, so response times are best effort. The maintainer will acknowledge a report, assess it, work on a fix for confirmed issues and credit you in the release notes if you wish. Please allow a reasonable time to ship a fix before disclosing the issue publicly.

## Scope

In scope:

- code in this repository that ships in the package,
- ways to make a control or the designer run code or load a type that the application did not ask for, for example through a crafted layout file,
- ways to make a control read files or use the network when it should not,
- XML parsing problems in layout or settings files, such as external entity loading.

Out of scope:

- issues that need an already compromised computer or Visual Studio installation,
- vulnerabilities in .NET, Windows or Visual Studio,
- findings that depend on a modified copy of the package.
