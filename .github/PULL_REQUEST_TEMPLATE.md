## Summary

<!-- What does this change and why? Keep it to one topic. -->

## Related issue

<!-- For example "Fixes #123". For a small typo or fix, write "None". -->

## Type of change

- [ ] Bug fix
- [ ] New feature
- [ ] Designer support
- [ ] Documentation
- [ ] Maintenance or CI

## How I checked it

<!-- Say what you ran, such as the smoke test, and what you tried by hand. Include your Windows and Visual Studio versions and one failure case you tried. -->

## Screenshots

<!-- Optional. Add before and after images for visible changes. -->

## Checklist

- [ ] `dotnet build Mordex.sln -c Release` finishes with no warnings
- [ ] `dotnet run --project demo/Mordex.Demo -c Release --no-build -- --smoke` passes
- [ ] The change is focused and does one thing
- [ ] I tried the change in a real form, including a failure case (or I explained why not)
- [ ] Commit messages follow the Conventional Commits style
- [ ] Objects that need disposing are disposed
- [ ] User-visible changes are recorded in `CHANGELOG.md`
- [ ] I wrote this change or I have the right to submit it under the MIT License
