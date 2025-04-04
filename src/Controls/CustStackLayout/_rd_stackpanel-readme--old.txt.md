
---

## README.md (Updated Draft)

# WinForms StackLayout Control

A custom WinForms Panel that arranges child controls sequentially in a single line (horizontally or vertically), with advanced features like weighted expansion and relative positioning of "floating" elements.

## Features

*   **Orientation:** Stack children `Horizontal` or `Vertical` (`lay_Orientation`).
*   **Spacing:** Define fixed pixel spacing between adjacent flow controls (`lay_Spacing`).
*   **Child Alignment (Cross Axis):** Align flow controls (`Start`, `Center`, `End`) or `Stretch` them perpendicular to the stacking orientation (`lay_ChildAxisAlignment`).
*   **Weighted Expansion (Main Axis):** Allow specific *flow* children to expand and fill remaining space along the stacking orientation based on a relative weight (`lay_ExpandWeight`).
*   **Floating Controls:** Designate specific children to be excluded from the flow layout and positioned relative to another "target" control (`lay_IsFloating`, `lay_FloatTargetName`, `lay_FloatOffsetX`, `lay_FloatOffsetY`).
*   **Float Alignment:** Control how a floating control aligns to its target (`lay_FloatAlignment`: `TopLeft`, `ToLeftOf`, `ToRightOf`) before offsets are applied.
*   **Float Z-Order:** Floating controls are automatically placed *behind* their target control in the Z-order.
*   **Multiple Layout Methods:** Choose different calculation methods (`lay_PerformLayout_calcMethod_No`: `0` for designer flexibility with weights, `4` for strict weight-based distribution).
*   **Manual Extender Linking:** Requires manual code assignment for runtime reliability (`LayoutExtenderProvider`).
*   **Designer Integration:** Most properties visible in the designer under the `L_Layout2` category.
*   **AutoScroll Support:** Basic integration with Panel's `AutoScroll` (calculated based on *flow* controls).

## How to Use

1.  **Add Code:** Include `StackLayout.cs` and `StackLayoutExtender.cs` in your project (ensure the namespace matches or update it).
2.  **Build:** Rebuild your project/solution (Ctrl+Shift+B).
3.  **Add StackLayout:** Drag the `StackLayout` control from the Visual Studio Toolbox onto your Form.
4.  **Add Extender Component:** Drag the `StackLayoutExtender` component from the Toolbox (usually under "Components") onto the **same Form**. An instance (e.g., `stackLayoutExtender1`) will appear in the component tray.
5.  

Okay, bro, you're absolutely right! Since we made `LayoutExtenderProvider` browsable (`[Browsable(true)]`), the designer *should* indeed allow assigning it directly via the Properties window. The previous README text was too restrictive.

Here's the revised section for the README, reflecting that the designer assignment is also an option:


**IMPORTANT - Establish Extender Link:** For runtime features like `lay_ExpandWeight` and floating properties to function, the `StackLayout` panel requires a reference to the `StackLayoutExtender` component instance that is also on your form. **You MUST establish this link using one of the following methods:**

*   **Method 1 (Recommended - Designer):**
    1.  Select the `StackLayout` panel on your form.
    2.  In the **Properties Window**, find the `LayoutExtenderProvider` property (it should be under the `L_Layout2` category).
    3.  Click the dropdown arrow for this property.
    4.  Select the instance name of the `StackLayoutExtender` you added to the form (e.g., `stackLayoutExtender1`).

*   **Method 2 (Code):**
    1.  Alternatively, you can assign the link programmatically, typically in your form's `Load` or `Shown` event handler:
        ```csharp
        // In YourForm.cs code-behind
        private void YourForm_Load(object sender, EventArgs e) // Or YourForm_Shown
        {
            // Replace with your actual control names if different
            this.stackLayout1.LayoutExtenderProvider = this.stackLayoutExtender1;
        }
        ```

*Failure to establish this link using either the designer property window or code will prevent `lay_ExpandWeight` and floating control features from working correctly at runtime.*

 
6.  **Add Children:** Drag other controls (Buttons, TextBoxes, Panels, etc.) *inside* the `StackLayout` panel.
7.  **Configure StackLayout Properties:**
    *   Select the `StackLayout` panel and set its properties in the Properties window (most under `L_Layout2` category):
        *   `lay_Orientation`: `Horizontal` or `Vertical`.
        *   `lay_Spacing`: Pixels between flow controls.
        *   `lay_ChildAxisAlignment`: `Stretch` (default), `Start`, `Center`, or `End`.
        *   `lay_PerformLayout_calcMethod_No`: `0` (flexible) or `4` (strict weight).
        *   `Padding`, `Size`, `Dock`, `Anchor` (Standard panel properties, also in `L_Layout2`).
        *   `AutoSize`: **Set to `False`** if you want children to expand using `lay_ExpandWeight`.
8.  **Configure Child Control Properties:**
    *   Select a child control *inside* the `StackLayout`:
    *   **For Expansion:**
        *   Find `lay_ExpandWeight on [extenderName]` under `L_Layout2`.
        *   Set to `0` (default) for fixed size, or `1+` to expand proportionally with other weighted *flow* controls.
    *   **For Floating:**
        *   Find `lay_IsFloating on [extenderName]`: Set to `True`.
        *   Find `lay_FloatTargetName on [extenderName]`: Enter the **exact `Name` property** of the *sibling* control this one should follow. Leave empty to position relative to panel padding.
        *   Find `lay_FloatAlignment on [extenderName]`: Choose `TopLeft` (default), `ToLeftOf`, or `ToRightOf`.
        *   Find `lay_FloatOffsetX on [extenderName]`: Horizontal pixel offset.
        *   Find `lay_FloatOffsetY on [extenderName]`: Vertical pixel offset.

## Key Properties

*   **On `StackLayout`:**
    *   `LayoutExtenderProvider`: `StackLayoutExtender` (MUST be set in code).
    *   `lay_Orientation`: `Vertical` / `Horizontal`
    *   `lay_Spacing`: `int` (pixels)
    *   `lay_ChildAxisAlignment`: `Stretch` / `Start` / `Center` / `End`
    *   `lay_PerformLayout_calcMethod_No`: `0` / `4`
    *   `Padding`, `AutoScroll`, `Size`, `Dock`, `Anchor` (Standard Panel properties)
*   **On Child Controls (provided by `StackLayoutExtender`):**
    *   `lay_ExpandWeight on [extenderName]`: `int` (0 = no expand, >0 = expand proportionally) - *Applies only if `lay_IsFloating` is False*.
    *   `lay_IsFloating on [extenderName]`: `bool` (True to enable floating).
    *   `lay_FloatTargetName on [extenderName]`: `string` (Name of sibling target control).
    *   `lay_FloatAlignment on [extenderName]`: `FloatAlignment` (TopLeft, ToLeftOf, ToRightOf).
    *   `lay_FloatOffsetX on [extenderName]`: `int` (pixels).
    *   `lay_FloatOffsetY on [extenderName]`: `int` (pixels).

## Floating Controls Explained

*   Set `lay_IsFloating = True` to remove a control from the normal stack flow.
*   Set `lay_FloatTargetName` to the `Name` of another control *within the same StackLayout* to position relative to it. If empty or invalid, the floater positions relative to the StackLayout's padding edge.
*   `lay_FloatAlignment` determines the base alignment point on the target (`TopLeft`, `ToLeftOf`, `ToRightOf`) before offsets are applied.
*   `lay_FloatOffsetX/Y` apply pixel offsets from the alignment point.
*   Floating controls are automatically placed *behind* their specified target control in the Z-order. Untargeted floaters are sent to the back.

## Troubleshooting / Notes

*   **Weights/Floating Not Working?** **CRITICAL:** Ensure you have manually assigned the extender instance to the `LayoutExtenderProvider` property in your form's code (`Form_Load` or `Form_Shown`).
*   **Designer Property Weirdness?** Due to the manual `LayoutExtenderProvider` link, the extender properties (`lay_ExpandWeight`, `lay_IsFloating`, etc.) on child controls *might* not always behave perfectly in the designer (e.g., not appearing reliably or changes not saving). Runtime behavior depends on the manual link. Set properties carefully and verify in `Form.Designer.cs`.
*   **Expansion Not Working?** Check `StackLayout.AutoSize = False`, sufficient panel size, `lay_ExpandWeight > 0` on a *flow* control, and target control's `MaximumSize`.
*   **Float Target Not Found:** If `lay_FloatTargetName` is empty, misspelled, or the target control is hidden (`Visible=False`), the floater will position relative to the `StackLayout`'s top-left padding edge.
*   **Layout Method 4:** Strictly enforces calculated sizes for expanding controls, overriding manual designer resizing. Method 0 is more flexible.

---

## Notes From Our Session (Internal Reference)

*   **Manual Extender Link:** Implemented `StackLayout.LayoutExtenderProvider` property. This requires users to manually assign the `StackLayoutExtender` instance in Form code (e.g., `Form_Load`). This was done to fix runtime issues where the automatic `Site`/`Container` based lookup (`FindExtender`) was unreliable (`extender` was often `null`). **Consequence:** Potential impact on design-time reliability of extender properties showing/saving on child controls. Runtime is now dependent on the manual link.
*   **`lay_` Prefix:** Applied `lay_` prefix to all custom `StackLayout` properties and extender-provided properties for organization/namespacing.
*   **Refactoring:** `PerformStackLayout_old_v0` and `PerformStackLayout_v4` were refactored into smaller helper functions (`PL_pX__...`) for better structure and clarity.
*   **Layout Method 1 Removed:** `PerformStackLayout_v1` was deleted due to observed issues (collapsing controls). Only Methods 0 and 4 remain.
*   **Floating Controls Added:**
    *   Implemented via extender properties: `lay_IsFloating`, `lay_FloatTargetName`, `lay_FloatOffsetX`, `lay_FloatOffsetY`, `lay_FloatAlignment`.
    *   Targeting uses control `Name` (string). Fallback positions relative to padding.
    *   `FloatAlignment` enum added (`TopLeft`, `ToLeftOf`, `ToRightOf`).
    *   Z-order logic added to place floaters *behind* their specific target, or sent to back if untargeted.
*   **`_isPerformingLayout` Flag:** User chose to preserve the version where `_isPerformingLayout = false;` is called within `PL_p2__Handle_Case_of_No_Flow_Controls`. Standard practice would be to *only* reset this in the `finally` block of the main layout methods (`v0`/`v4`). Keeping it this way carries a *potential risk* of re-entrancy if the early exit path in `PL_p2__` doesn't fully unwind the call stack as expected by other UI events. Monitor if necessary.
*   **Code Style:** Agreed to use standard C# pretty-printing (braces on separate lines, standard indentation).
*   **`Dock`/`Anchor` Visibility:** These standard properties were made visible again and added to the `L_Layout2` category.

---

 
## Visual Examples (Using SharpBrowser UI)

**(Note:** You need to replace the `![Alt text](path)` lines below with your actual screenshots. Create an `images` folder in your repository/project and save your screenshots there for cleanliness.)*

**1. Vertical Stacking with Spacing**
*   Shows controls arranged vertically with `lay_Spacing` applied between them.

    ```markdown
    ![Vertical StackLayout with Spacing](images/example_vertical_stack_spacing.png)
    ```
    *(Caption: Example of controls stacked vertically in SharpBrowser's UI using StackLayout)*

**2. Horizontal Stacking with Alignment**
*   Demonstrates controls arranged horizontally. Pay attention to `lay_ChildAxisAlignment` (e.g., `Stretch` vs `Center`).

    ```markdown
    ![Horizontal StackLayout with Stretch Alignment](images/example_horizontal_stretch.png)
    ```
    *(Caption: Example of horizontal stack with `lay_ChildAxisAlignment = Stretch`)*

    ```markdown
    ![Horizontal StackLayout with Center Alignment](images/example_horizontal_center.png)
    ```
    *(Caption: Example of horizontal stack with `lay_ChildAxisAlignment = Center`)*

**3. Weighted Expansion**
*   Shows a control (e.g., a TextBox or Panel) with `lay_ExpandWeight > 0` filling the available space in the stack direction.

    ```markdown
    ![StackLayout with Weighted Expansion](images/example_weighted_expansion.png)
    ```
    *(Caption: A control expanding to fill space due to `lay_ExpandWeight`)*

**4. Floating Control (Relative to Target)**
*   Illustrates a control (`lay_IsFloating = True`) positioned relative to another control (`lay_FloatTargetName`) using `lay_FloatAlignment` (e.g., `ToRightOf`) and offsets.

    ```markdown
    ![Floating Control next to Target](images/example_floating_relative.png)
    ```
    *(Caption: A floating button positioned to the right of another control using `lay_FloatTargetName` and `lay_FloatAlignment`)*

**5. Floating Control (Relative to Panel)**
*   Shows a floating control positioned using only offsets (`lay_FloatTargetName` is empty), relative to the panel's padding.

    ```markdown
    ![Floating Control relative to Panel](images/example_floating_absolute.png)
    ```
    *(Caption: A floating element positioned relative to the StackLayout panel itself)*

--- 

**Next Steps for You:**

1.  Create an `images` folder in your project directory (or wherever you keep your README).
2.  Run SharpBrowser and take screenshots demonstrating the features listed in the "Visual Examples" section.
3.  Save these screenshots with descriptive names (like `example_vertical_stack_spacing.png`) into the `images` folder.
4.  Edit the `README.md` file and replace the placeholder lines like `![Vertical StackLayout with Spacing](images/example_vertical_stack_spacing.png)` with the correct paths if your filenames or folder structure differ.
5.  Commit the updated README and the new `images` folder to your repository.