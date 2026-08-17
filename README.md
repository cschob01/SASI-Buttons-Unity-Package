SASI Buttons Unity Package
Carlos Schober 
8/17/2026

Tested Unity Version(s): 6.3

Static and Singleton-Integrated (SASI) Buttons are meant as a quality-of-life 
improvement for Unity projects. The usual OnClick() editor menu for UI buttons
works off of class instances, and thus does not permit calling static class
functions. This package modifies the editor to include an extra OnClick() menu
specifically for calls to singleton and static classes.

Notes:
  * SAS classes to be wired to SASI Buttons should be marked with
    [SASIButtonsCallable].
  * SASI Buttons support SAS functions with up to one parameter, and for most
    parameter types will provide convenient serialization.
  * SASI Buttons store SAS calls via string paths, which are reflected to find
    the corresponding methods. If a link is broken (likely due to renaming of
    classes/methods) the user will be notified.
  * See [YOUTUBE VIDEO HERE] for package demonstration.

