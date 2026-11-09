export interface AppTheme {
  dark: boolean
  colors: Readonly<Record<string, string>>
  variables: Readonly<Record<string, string | number>>
}

export const lightTheme: AppTheme = {
  dark: false,
  colors: {
    background: '#FFFFFF',
    surface: '#FFFFFF',
    'surface-light': '#F5F5F5',
    primary: '#594AE2',
    secondary: '#FF4081',
    tertiary: '#1EC8A5',
    info: '#2196F3',
    success: '#00C853',
    warning: '#FF9800',
    error: '#F44336',
    'on-background': '#424242',
    'on-surface': '#424242',
    'on-tertiary': '#FFFFFF',
    'on-success': '#FFFFFF',
    'on-warning': '#FFFFFF',
  },
  variables: {
    'high-emphasis-opacity': 1,
    'medium-emphasis-opacity': 0.73,
    'disabled-opacity': 0.513,
    'border-color': '#000000',
    'border-opacity': 0.12,
  },
}

export const darkTheme: AppTheme = {
  dark: true,
  colors: {
    background: '#32333D',
    surface: '#373740',
    'surface-light': '#27272F',
    primary: '#776BE7',
    secondary: '#FF4081',
    tertiary: '#1EC8A5',
    info: '#3299FF',
    success: '#0BBA83',
    warning: '#FFA800',
    error: '#F64E62',
    'on-background': '#FFFFFF',
    'on-surface': '#FFFFFF',
    'on-tertiary': '#FFFFFF',
    'on-success': '#FFFFFF',
    'on-warning': '#FFFFFF',
  },
  variables: {
    'high-emphasis-opacity': 0.7,
    'medium-emphasis-opacity': 0.56,
    'disabled-opacity': 0.2,
    'border-color': '#FFFFFF',
    'border-opacity': 0.12,
  },
}
