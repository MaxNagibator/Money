export type MudColor =
  | 'default'
  | 'primary'
  | 'secondary'
  | 'tertiary'
  | 'info'
  | 'success'
  | 'warning'
  | 'error'
  | 'dark'
  | 'transparent'
  | 'inherit'
  | 'surface'

export type MudSize = 'small' | 'medium' | 'large'

export type MudVariant = 'text' | 'filled' | 'outlined'

export type MudTypo =
  | 'inherit'
  | 'h1'
  | 'h2'
  | 'h3'
  | 'h4'
  | 'h5'
  | 'h6'
  | 'subtitle1'
  | 'subtitle2'
  | 'body1'
  | 'body2'
  | 'button'
  | 'caption'
  | 'overline'

export type MudAlign = 'inherit' | 'left' | 'center' | 'right' | 'justify' | 'start' | 'end'

export type MudJustify = 'start' | 'center' | 'end' | 'space-between' | 'space-around' | 'space-evenly'

export type MudAlignItems = 'baseline' | 'center' | 'start' | 'end' | 'stretch'

export type MudWrap = 'nowrap' | 'wrap' | 'wrap-reverse'

export type MudStretchItems = 'none' | 'start' | 'end' | 'middle' | 'all'

export type MudBreakpoint = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | 'xxl'

export type MudSeverity = 'normal' | 'info' | 'success' | 'warning' | 'error'

export type MudSkeletonType = 'text' | 'circle' | 'rectangle'

export type MudAnimation = 'false' | 'pulse' | 'wave'

export type MudHorizontalAlignment = 'center' | 'left' | 'right' | 'start' | 'end'

export type MudNavLinkMatch = 'prefix' | 'all'

export type MudMaxWidth = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | 'xxl' | 'false'

export type MudTabHeaderPosition = 'before' | 'after'

export type MudSelectionMode = 'single-selection' | 'multi-selection' | 'toggle-selection'

export type MudButtonType = 'button' | 'submit' | 'reset'

export type MudEdge = 'false' | 'start' | 'end'

export type MudTimelineOrientation = 'vertical' | 'horizontal'

export type MudTimelinePosition = 'alternate' | 'top' | 'bottom' | 'left' | 'right' | 'start' | 'end'

export type MudOrigin =
  | 'top-left'
  | 'top-center'
  | 'top-right'
  | 'center-left'
  | 'center-center'
  | 'center-right'
  | 'bottom-left'
  | 'bottom-center'
  | 'bottom-right'

export interface MudTreeItemData {
  value?: unknown
  text?: string | undefined
  icon?: string | undefined
  iconExpanded?: string | undefined
  iconColor?: MudColor | undefined
  endIcon?: string | undefined
  endIconColor?: MudColor | undefined
  expanded?: boolean | undefined
  visible?: boolean | undefined
  disabled?: boolean | undefined
  children?: readonly MudTreeItemData[] | undefined
}

export type MudOverflowBehavior = 'flip-never' | 'flip-onopen' | 'flip-always'

export type MudDropdownWidth = 'relative' | 'adaptive' | 'ignore'

export type MudPlacement = 'left' | 'right' | 'end' | 'start' | 'top' | 'bottom'

export type MudMargin = 'none' | 'dense' | 'normal'

export type MudAdornment = 'none' | 'start' | 'end'

export type MudInputSizing = 'fixed' | 'auto'

export type MudInputMode = 'none' | 'text' | 'decimal' | 'numeric' | 'tel' | 'search' | 'email' | 'url'

export type MudInputType =
  | 'text'
  | 'password'
  | 'email'
  | 'hidden'
  | 'number'
  | 'search'
  | 'tel'
  | 'url'
  | 'color'
  | 'date'
  | 'datetime-local'
  | 'month'
  | 'time'
  | 'week'

export type MudPickerVariant = 'inline' | 'dialog' | 'static'

export type MudColorPickerMode = 'rgb' | 'hsl' | 'hex'
