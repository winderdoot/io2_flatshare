export const locationConfig = {
  warszawa: [
    "Śródmieście",
    "Mokotów",
    "Żoliborz",
    "Wola",
    "Praga-Północ",
    "Praga-Południe",
    "Ursynów",
  ],
  krakow: ["Stare Miasto", "Kazimierz", "Podgórze", "Nowa Huta", "Krowodrza"],
  wroclaw: ["Śródmieście", "Krzyki", "Psie Pole", "Fabryczna", "Stare Miasto"],
} as const;

export type City = keyof typeof locationConfig;

/** Canonical city names stored in the database. */
export const cityCanonicalNames: Record<City, string> = {
  warszawa: "Warszawa",
  krakow: "Kraków",
  wroclaw: "Wrocław",
};

const cityAliasToKey: Record<string, City> = {
  warszawa: "warszawa",
  warsaw: "warszawa",
  krakow: "krakow",
  kraków: "krakow",
  cracow: "krakow",
  wroclaw: "wroclaw",
  wrocław: "wroclaw",
};

const districtLabels: Record<City, Record<string, { pl: string; en: string }>> = {
  warszawa: {
    "Śródmieście": { pl: "Śródmieście", en: "City Centre" },
    Mokotów: { pl: "Mokotów", en: "Mokotów" },
    Żoliborz: { pl: "Żoliborz", en: "Żoliborz" },
    Wola: { pl: "Wola", en: "Wola" },
    "Praga-Północ": { pl: "Praga-Północ", en: "Praga-North" },
    "Praga-Południe": { pl: "Praga-Południe", en: "Praga-South" },
    Ursynów: { pl: "Ursynów", en: "Ursynów" },
  },
  krakow: {
    "Stare Miasto": { pl: "Stare Miasto", en: "Old Town" },
    Kazimierz: { pl: "Kazimierz", en: "Kazimierz" },
    Podgórze: { pl: "Podgórze", en: "Podgórze" },
    "Nowa Huta": { pl: "Nowa Huta", en: "Nowa Huta" },
    Krowodrza: { pl: "Krowodrza", en: "Krowodrza" },
  },
  wroclaw: {
    "Śródmieście": { pl: "Śródmieście", en: "City Centre" },
    Krzyki: { pl: "Krzyki", en: "Krzyki" },
    "Psie Pole": { pl: "Psie Pole", en: "Psie Pole" },
    Fabryczna: { pl: "Fabryczna", en: "Fabryczna" },
    "Stare Miasto": { pl: "Stare Miasto", en: "Old Town" },
  },
};

export function resolveCityKey(value: string): City | null {
  const normalized = value.trim().toLowerCase();
  if (!normalized) return null;
  return cityAliasToKey[normalized] ?? null;
}

export function cityApiName(cityKey: City): string {
  return cityCanonicalNames[cityKey];
}

export function cityDisplayName(cityKey: City, language: string): string {
  const isEn = language.startsWith("en");
  if (isEn) {
    return (
      { warszawa: "Warsaw", krakow: "Krakow", wroclaw: "Wroclaw" } as const
    )[cityKey];
  }
  return cityCanonicalNames[cityKey];
}

export function districtDisplayName(
  cityKey: City,
  district: string,
  language: string
): string {
  const labels = districtLabels[cityKey]?.[district];
  if (!labels) return district;
  return language.startsWith("en") ? labels.en : labels.pl;
}

export function citiesEquivalent(a: string, b: string): boolean {
  const keyA = resolveCityKey(a);
  const keyB = resolveCityKey(b);
  if (keyA && keyB) return keyA === keyB;
  return a.trim().toLowerCase() === b.trim().toLowerCase();
}

export function formatLocationCity(city: string, language: string): string {
  const key = resolveCityKey(city);
  if (key) return cityDisplayName(key, language);
  return city;
}

export function formatLocationDistrict(
  city: string,
  district: string,
  language: string
): string {
  const key = resolveCityKey(city);
  if (key) return districtDisplayName(key, district, language);
  return district;
}

/** @deprecated Use cityDisplayName(cityKey, language) instead. */
export const cityOptions = cityCanonicalNames;
