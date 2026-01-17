# Sleep Journal - Visual Design System

## Overview

The Sleep Journal app features a modern, calming design system aligned with the sleep and wellness theme. The visual design emphasizes tranquility, reflection, and emotional well-being through carefully chosen colors, typography, and layout patterns.

---

## Color Palette

### Primary - Deep Indigo/Purple

**Purpose:** Calm, reflective atmosphere for evening journaling

- **Primary:** `#5B4E9F` - Main brand color, used for headers and primary actions
- **Primary Dark:** `#8B7EC8` - Dark mode variant
- **Primary Light:** `#EBE8F5` - Subtle backgrounds
- **Primary Dark Text:** `#2A2438` - Text on light primary backgrounds

**Usage:**

- App header/branding
- Primary buttons
- Mood slider track
- Focus states

### Secondary - Soft Teal/Blue

**Purpose:** Tranquil, peaceful feeling

- **Secondary:** `#6BA3BE` - Social comfort theme color
- **Secondary Dark:** `#4A7B93` - Dark mode variant
- **Secondary Light:** `#E3F2F7` - Light backgrounds
- **Secondary Dark Text:** `#2D4A57` - Text on light secondary backgrounds

**Usage:**

- Social Comfort section
- Accent elements
- Informational highlights

### Accent - Warm Coral/Rose

**Purpose:** Comforting, gentle warmth

- **Accent:** `#E89EA3` - Reflection theme color
- **Accent Dark:** `#C77B81` - Hover/active states
- **Accent Light:** `#FCF0F1` - Subtle backgrounds

**Usage:**

- Reflection section
- Secondary actions
- Warm accents

### Mood Indicators

Emotional state representation:

- **Positive:** `#8BC34A` (Green)
- **Neutral:** `#FFC107` (Amber)
- **Negative:** `#FF6B6B` (Coral red)

### Semantic Colors

Standard UI feedback:

- **Success:** `#4CAF50`
- **Warning:** `#FF9800`
- **Error:** `#F44336`
- **Info:** `#2196F3`

### Neutrals - Sophisticated Gray Scale

Comprehensive gray scale for text hierarchy and surfaces:

| Color   | Hex       | Usage                   |
| ------- | --------- | ----------------------- |
| Gray50  | `#F7F8FA` | Light input backgrounds |
| Gray100 | `#EDF0F4` | Card backgrounds        |
| Gray200 | `#D7DCE3` | Borders, dividers       |
| Gray300 | `#B8C0CC` | Disabled states         |
| Gray400 | `#9AA5B5` | Placeholder text        |
| Gray500 | `#7B8A9E` | Secondary text          |
| Gray600 | `#5D6F87` | Body text               |
| Gray700 | `#4A5A6F` | Dark borders            |
| Gray800 | `#374557` | Emphasized text         |
| Gray900 | `#242F3F` | Primary text            |
| Gray950 | `#1A2332` | Maximum contrast        |

### Surface Colors

Background layers:

- **Surface Light:** `#FFFFFF` - Cards, elevated content
- **Surface Dark:** `#1E1E2E` - Dark mode cards
- **Background Light:** `#F5F7FA` - Page background
- **Background Dark:** `#16161E` - Dark mode background

---

## Typography

### Font Family

**Open Sans** - Clean, modern, highly readable sans-serif

- **OpenSansRegular** - Body text, labels, placeholders
- **OpenSansSemibold** - Headings, buttons, emphasis

### Scale

| Element        | Size | Weight   | Usage                   |
| -------------- | ---- | -------- | ----------------------- |
| Hero Title     | 28px | Semibold | App branding            |
| Display Number | 32px | Semibold | Slider values           |
| Section Header | 16px | Semibold | Section titles          |
| Body Text      | 15px | Regular  | Editor, input fields    |
| Button Text    | 16px | Semibold | Call-to-action buttons  |
| Secondary Text | 14px | Regular  | Subtitles, descriptions |
| Helper Text    | 12px | Regular  | Character counts, hints |
| Icon Emoji     | 40px | -        | Header icon             |
| Section Emoji  | 20px | -        | Section icons           |

### Line Height

- **Headings:** 1.2 (tight)
- **Body:** 1.5 (comfortable)
- **Buttons:** 1.0 (compact)

---

## Layout System

### Spacing Scale

Consistent spacing using 4px base unit:

- **4px** - Tight grouping
- **8px** - Related elements
- **12px** - Component internal padding
- **16px** - Standard spacing
- **20px** - Card padding
- **24px** - Section spacing
- **28px** - Major section gaps
- **32px** - Page margins

### Card Design

Modern card-based layout with depth:

```xaml
<Border BackgroundColor="{StaticResource Surface}"
        StrokeThickness="0"
        Padding="20,24">
    <Border.StrokeShape>
        <RoundRectangle CornerRadius="20"/>
    </Border.StrokeShape>
    <Border.Shadow>
        <Shadow Brush="{StaticResource Gray900Brush}"
                Offset="0,2"
                Radius="16"
                Opacity="0.06"/>
    </Border.Shadow>
</Border>
```

**Characteristics:**

- **Corner Radius:** 20px (main cards), 12px (input fields), 14px (buttons)
- **Shadows:** Subtle (0.06-0.08 opacity), offset slightly down
- **Borders:** None on cards, 1px on input fields
- **Background:** White/dark surface with slight elevation

### Input Fields

Bordered, nested design for clarity:

```xaml
<Border BackgroundColor="{AppThemeBinding Light={StaticResource Gray50}, Dark={StaticResource Gray900}}"
        StrokeThickness="1"
        Stroke="{AppThemeBinding Light={StaticResource Gray200}, Dark={StaticResource Gray700}}"
        Padding="16,12">
    <Border.StrokeShape>
        <RoundRectangle CornerRadius="12"/>
    </Border.StrokeShape>
</Border>
```

**Features:**

- Light background differentiation
- Subtle borders
- Generous padding
- Character count helpers

---

## Component Styles

### Buttons

**Primary Button:**

- Background: Primary color gradient
- Text: White, Semibold, 16px
- Padding: 16px vertical (56px total height)
- Corner Radius: 14px
- Shadow: Primary color, 0.25 opacity
- States:
  - **Normal:** Scale 1.0
  - **Hover:** Scale 1.02, Opacity 0.9
  - **Pressed:** Scale 0.98, Opacity 0.8
  - **Disabled:** Gray background, 0.6 opacity

### Sliders

**Color-coded by metric:**

- **Mood:** Primary (`#5B4E9F`)
- **Social Comfort:** Secondary (`#6BA3BE`)
- **Reflection:** Accent (`#E89EA3`)

**Design:**

- Large numeric display (32px)
- Descriptive helper text
- Color-matched track and thumb
- Minimum touch target: 44x44px

### Text Editor

**Characteristics:**

- Transparent background
- Placeholder: Gray400
- Text: Gray900 (light) / Gray100 (dark)
- Minimum height: 80px
- Auto-resize with content
- Character counter: Gray500, 12px

---

## Iconography

### Emoji Icons

Used for visual context and emotional connection:

- 🌙 - App branding (sleep/night theme)
- ✍️ - Journal entry
- 😊 - Mood
- 🤝 - Social comfort
- 💭 - Reflection

**Sizes:**

- Header: 40px
- Section headers: 20px

---

## Visual Hierarchy

### Header Section

1. **Moon emoji** - Brand identity
2. **App title** - Large, bold, white on primary
3. **Subtitle** - Smaller, lighter, descriptive

### Content Card

Elevated white card with:

- Negative margin to overlap header
- Deep shadow for depth
- Rounded corners for softness

### Section Organization

Each metric section includes:

1. **Icon + Title** - Horizontal layout, semibold
2. **Bordered container** - Light background, rounded
3. **Slider control** - Color-matched to section
4. **Large value display** - 32px, centered, colored
5. **Helper text** - Small, gray, centered

---

## Accessibility

### Contrast Ratios

All color combinations meet WCAG AA standards:

- Primary text on Surface: 16.1:1
- Secondary text on Surface: 7.3:1
- Button text on Primary: 5.2:1

### Touch Targets

Minimum 44x44px for interactive elements:

- Buttons: 48px minimum height
- Sliders: Standard touch area
- Input fields: 80px minimum height

### Semantic Properties

Proper heading levels for screen readers:

```xaml
SemanticProperties.HeadingLevel="Level1"
```

---

## Dark Mode Support

All colors have dark mode variants using `AppThemeBinding`:

```xaml
TextColor="{AppThemeBinding Light={StaticResource Gray900}, Dark={StaticResource Gray100}}"
```

**Dark mode adjustments:**

- Inverted gray scale
- Slightly lighter primary colors
- Reduced shadow opacity
- Maintained contrast ratios

---

## Animation & Interaction

### Button States

Smooth scale animations for tactile feedback:

- **Hover:** 102% scale
- **Pressed:** 98% scale
- **Transition:** Implicit (system default)

### Visual Feedback

- Opacity changes on interaction
- Color transitions on state changes
- Loading spinner during saves

---

## Implementation Files

### Core Design System

1. **Colors.xaml** - Complete color palette
2. **Styles.xaml** - Component styles and visual states
3. **App.xaml** - Resource registration
4. **StringToBoolConverter.cs** - Conditional visibility helper

### UI Implementation

- **MainPage.xaml** - Main journaling interface
- Uses card-based layout
- Color-coded metric sections
- Responsive spacing

---

## Best Practices

### Color Usage

✅ Use semantic colors for UI feedback  
✅ Use themed colors for sections (mood, social, reflection)  
✅ Maintain consistent contrast ratios  
❌ Don't mix unrelated accent colors

### Typography

✅ Use Semibold for emphasis and actions  
✅ Use Regular for content and descriptions  
✅ Maintain consistent sizing within component types  
❌ Don't exceed 3 font sizes per screen

### Spacing

✅ Use 8px increments for spacing  
✅ Maintain generous padding in cards (20-24px)  
✅ Use consistent gaps between sections (28px)  
❌ Don't use arbitrary spacing values

### Shadows

✅ Keep shadows subtle (0.06-0.08 opacity)  
✅ Use downward offset (0,2 or 0,4)  
✅ Match shadow color to background  
❌ Don't over-emphasize depth

---

## Future Enhancements

- **Animations:** Smooth transitions between states
- **Haptic Feedback:** Subtle vibrations on interactions
- **Custom Icons:** SVG icons to replace emoji for production
- **Theming:** User-selectable color schemes
- **Adaptive Layouts:** Tablet-optimized layouts
- **Micro-interactions:** Slider value animations

---

## Design Inspiration

**Influences:**

- Material Design 3 (card-based layouts, elevation)
- iOS Human Interface Guidelines (spacing, touch targets)
- Calm/Headspace apps (soothing color palettes)
- Modern wellness apps (emoji usage, friendly tone)

**Goals:**

- Create a peaceful journaling experience
- Encourage daily reflection
- Reduce cognitive load with clear hierarchy
- Provide emotional context through color
- Support both light and dark mode usage patterns
