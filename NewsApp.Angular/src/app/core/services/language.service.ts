import { Injectable } from '@angular/core';
import { LanguageOption } from '../../shared/models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  
  // ✅ Lista de idiomas soportados por NewsAPI
  private readonly supportedLanguages: LanguageOption[] = [
    { code: 'en', name: 'English', flag: '🇺🇸' },
    { code: 'es', name: 'Español', flag: '🇪🇸' },
    { code: 'fr', name: 'Français', flag: '🇫🇷' },
    { code: 'de', name: 'Deutsch', flag: '🇩🇪' },
    { code: 'it', name: 'Italiano', flag: '🇮🇹' },
    { code: 'pt', name: 'Português', flag: '🇵🇹' },
    { code: 'ru', name: 'Русский', flag: '🇷🇺' },
    { code: 'zh', name: '中文', flag: '🇨🇳' },
    { code: 'ja', name: '日本語', flag: '🇯🇵' },
    { code: 'ko', name: '한국어', flag: '🇰🇷' },
    { code: 'ar', name: 'العربية', flag: '🇸🇦' },
    { code: 'nl', name: 'Nederlands', flag: '🇳🇱' },
    { code: 'no', name: 'Norsk', flag: '🇳🇴' },
    { code: 'sv', name: 'Svenska', flag: '🇸🇪' },
    { code: 'da', name: 'Dansk', flag: '🇩🇰' },
    { code: 'he', name: 'עברית', flag: '🇮🇱' },
    { code: 'hi', name: 'हिन्दी', flag: '🇮🇳' }
  ];

  constructor() {}

  // Obtener todos los idiomas disponibles
  getAvailableLanguages(): LanguageOption[] {
    return this.supportedLanguages;
  }

  // Obtener información de un idioma específico
  getLanguageInfo(languageCode: string): LanguageOption | undefined {
    return this.supportedLanguages.find(lang => lang.code === languageCode);
  }

  // Obtener nombre del idioma
  getLanguageName(languageCode: string): string {
    const language = this.getLanguageInfo(languageCode);
    return language ? language.name : 'Unknown';
  }

  // Obtener bandera del idioma
  getLanguageFlag(languageCode: string): string {
    const language = this.getLanguageInfo(languageCode);
    return language ? language.flag : '🌍';
  }

  // Validar si un código de idioma es válido
  isValidLanguageCode(languageCode: string): boolean {
    return this.supportedLanguages.some(lang => lang.code === languageCode);
  }
}
