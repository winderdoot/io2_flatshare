import { create } from "zustand";
import { locationConfig } from "./locationConfig";
import { profileConfig } from "./profileConfig";

export type City = keyof typeof locationConfig;

export type Profile = keyof typeof profileConfig;
export type ProfileValue = Profile | "";

export interface Filters {
  city: City | "";
  district: string;
  minPrice: string;
  maxPrice: string;
  petsAllowed: boolean;
  nonSmokingOnly: boolean;
  closeToShops: boolean;
  profile: ProfileValue;
  minArea: string;
  maxArea: string;
  startDate: string;
}

const initialFilters: Filters = {
  city: "",
  district: "",
  minPrice: "",
  maxPrice: "",
  petsAllowed: false,
  nonSmokingOnly: false,
  closeToShops: false,
  profile: "",
  minArea: "",
  maxArea: "",
  startDate: ""
};

interface FiltersState {
  filters: Filters;
  appliedFilters: Filters;

  page: number;
  setFilter: <K extends keyof Filters>(key: K, value: Filters[K]) => void;
  setPage: (page: number) => void;

  applyFilters: () => void;
  resetFilters: () => void;
  initFromPreferences: (partial: Partial<Filters>) => void;
}

export const useFiltersStore = create<FiltersState>((set) => ({
  filters: initialFilters,
  appliedFilters: initialFilters,
  page: 0,

  setFilter: (key, value) =>
    set((state) => {
      const newFilters = {
        ...state.filters,
        [key]: value
      };

      return {
        filters: newFilters,
        page: 0
      };
    }),

  setPage: (page) => set({ page }),

  applyFilters: () =>
    set((state) => ({
      appliedFilters: state.filters,
      page: 0
    })),

  resetFilters: () =>
    set({
      filters: initialFilters,
      appliedFilters: initialFilters,
      page: 0
    }),

  initFromPreferences: (partial) =>
    set((state) => {
      const newFilters = { ...state.filters, ...partial };
      return {
        filters: newFilters,
        appliedFilters: newFilters,
        page: 0
      };
    })
}));