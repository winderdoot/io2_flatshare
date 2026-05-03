import { Link, useParams } from "react-router-dom";
import type { ListingDTO } from "../../models/listing";
import { findMockListing } from "./mockListings";
import "./ListingDetail.css";

function formatDate(iso: string): string {
  const d = new Date(iso + "T12:00:00");
  return new Intl.DateTimeFormat("pl-PL", {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(d);
}

function contactIsPhone(contact: string): boolean {
  return /^\+?[\d\s-]{9,}$/.test(contact.replace(/\s/g, ""));
}

function attributeChips(listing: ListingDTO) {
  const { attributes } = listing;
  const chips: { key: string; label: string; variant?: "accent" | "warn" }[] = [
    {
      key: "profile",
      label:
        attributes.profile === "Student"
          ? "Profil: studenci"
          : "Profil: turyści / krótki pobyt",
      variant: "accent",
    },
  ];
  if (attributes.petsAllowed) {
    chips.push({ key: "pets", label: "Zwierzęta dozwolone" });
  } else {
    chips.push({ key: "pets", label: "Bez zwierząt", variant: "warn" });
  }
  if (attributes.nonSmokingOnly) {
    chips.push({ key: "smoke", label: "Tylko dla niepalących" });
  }
  if (attributes.closeToShops) {
    chips.push({ key: "shops", label: "Blisko sklepów" });
  }
  return chips;
}

export const ListingDetail = () => {
  const { listingId } = useParams<{ listingId: string }>();
  const listing = listingId ? findMockListing(listingId) : undefined;

  if (!listingId || !listing) {
    return (
      <div className="listing-detail">
        <div className="listing-detail-empty">
          <p>Nie znaleziono ogłoszenia (mock — sprawdź identyfikator w URL).</p>
          <p>
            <Link to="/offer">Wróć do listy ofert</Link>
          </p>
        </div>
      </div>
    );
  }

  const addressLine = [
    listing.location.street,
    listing.location.aptNumber,
    listing.location.district,
    listing.location.city,
  ].join(", ");

  const chips = attributeChips(listing);
  const imageSrc = listing.coverImageUrl ?? "";

  return (
    <article className="listing-detail">
      <Link className="listing-detail-back" to="/offer">
        ← Wróć do ofert
      </Link>

      <div className="listing-detail-hero">
        {imageSrc ? (
          <img src={imageSrc} alt={listing.title} />
        ) : (
          <div aria-hidden style={{ width: "100%", height: "100%" }} />
        )}
        <div className="listing-detail-price-pill">
          {listing.price} {listing.currency} / mies.
        </div>
      </div>

      <header className="listing-detail-head">
        <h1>{listing.title}</h1>
        <p className="listing-detail-address">📍 {addressLine}</p>
      </header>

      <div className="listing-detail-grid">
        <section className="listing-detail-panel">
          <h2>Opis</h2>
          <p className="listing-detail-description">{listing.description}</p>
          <div className="listing-detail-attributes" aria-label="Atrybuty oferty">
            {chips.map((c) => (
              <span
                key={c.key}
                className={[
                  "listing-detail-chip",
                  c.variant === "accent" ? "listing-detail-chip--accent" : "",
                  c.variant === "warn" ? "listing-detail-chip--warn" : "",
                ]
                  .filter(Boolean)
                  .join(" ")}
              >
                {c.label}
              </span>
            ))}
          </div>
        </section>

        <aside className="listing-detail-panel">
          <h2>Najważniejsze</h2>
          <div className="listing-detail-facts">
            <div className="listing-detail-fact">
              <span className="listing-detail-fact-label">Powierzchnia</span>
              <p className="listing-detail-fact-value">
                {listing.area} m<sup>2</sup>
              </p>
            </div>
            <div className="listing-detail-fact">
              <span className="listing-detail-fact-label">Dostępne od</span>
              <p className="listing-detail-fact-value">
                {formatDate(listing.availableSince)}
              </p>
            </div>
            <div className="listing-detail-fact">
              <span className="listing-detail-fact-label">Dostępne do</span>
              <p className="listing-detail-fact-value">
                {formatDate(listing.availableUntil)}
              </p>
            </div>
          </div>
          <div className="listing-detail-contact">
            <div className="listing-detail-facts">
              <div className="listing-detail-fact">
                <span className="listing-detail-fact-label">
                  Kontakt właściciela
                </span>
                <p className="listing-detail-fact-value">
                  {contactIsPhone(listing.ownerContact) ? (
                    <a href={`tel:${listing.ownerContact.replace(/\s/g, "")}`}>
                      {listing.ownerContact}
                    </a>
                  ) : (
                    <a href={`mailto:${listing.ownerContact}`}>
                      {listing.ownerContact}
                    </a>
                  )}
                </p>
              </div>
            </div>
          </div>
        </aside>
      </div>
    </article>
  );
};
