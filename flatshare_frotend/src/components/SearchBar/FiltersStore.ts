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
  closeToShops: string;
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
  closeToShops: "",
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
}

export const useFiltersStore = create<FiltersState>((set) => ({
  filters: initialFilters,
  appliedFilters: initialFilters,
  page: 1,

  setFilter: (key, value) =>
    set((state) => {
      const newFilters = {
        ...state.filters,
        [key]: value
      };

      return {
        filters: newFilters,
        page: 1
      };
    }),

  setPage: (page) => set({ page }),

  applyFilters: () =>
    set((state) => ({
      appliedFilters: state.filters,
      page: 1
    })),

  resetFilters: () =>
    set({
      filters: initialFilters,
      appliedFilters: initialFilters,
      page: 1
    })
}));