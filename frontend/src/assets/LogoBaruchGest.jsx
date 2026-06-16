export default function LogoBaruchGest({ size = 40 }) {
  return (
    <svg width={size} height={size} viewBox="0 0 40 40" fill="none" xmlns="http://www.w3.org/2000/svg">
      <rect width="40" height="40" rx="10" fill="#0F172A"/>
      {/* Traço vertical do B */}
      <rect x="8" y="8" width="4" height="24" rx="1" fill="white"/>
      {/* Curva superior do B */}
      <path d="M12 8h5.5a4.5 4.5 0 0 1 0 9H12V8z" fill="white"/>
      {/* Curva inferior do B (maior) */}
      <path d="M12 17h6.5a5.5 5.5 0 0 1 0 11H12V17z" fill="white"/>
      {/* Barras de crescimento */}
      <rect x="25" y="23" width="3" height="9" rx="1.5" fill="#10B981"/>
      <rect x="29.5" y="18" width="3" height="14" rx="1.5" fill="#10B981"/>
      <rect x="34" y="13" width="3" height="19" rx="1.5" fill="#10B981"/>
      {/* Seta de tendência */}
      <polyline points="25,23 31,17 37,12" stroke="#10B981" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" fill="none"/>
      <polygon points="34,10 39,10 39,15" fill="#10B981"/>
    </svg>
  );
}
